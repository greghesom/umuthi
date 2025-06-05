using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Functions.Helpers;

namespace umuthi.Functions.Services;

/// <summary>
/// Service for speech-to-text transcription using Azure Cognitive Services
/// </summary>
public class SpeechTranscriptionService : ISpeechTranscriptionService
{
    public async Task<object> TranscribeAudioFilesAsync(
        IEnumerable<IFormFile> audioFiles, 
        string language, 
        bool includeTimestamps, 
        ILogger logger)
    {
        List<(string TempPath, string FileName)> tempFiles = new();
        List<string> wavFiles = new();

        try
        {
            // Save all uploaded files to temporary locations
            foreach (var file in audioFiles)
            {
                var tempPath = Path.GetTempFileName();
                tempFiles.Add((tempPath, file.FileName));
                
                // Save uploaded file to temp location
                using (var fileStream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                
                logger.LogInformation($"Saved file: {file.FileName}, Size: {file.Length} bytes to {tempPath}");
            }

            // Convert files to WAV format for speech recognition
            foreach (var (tempPath, fileName) in tempFiles)
            {
                var wavPath = Path.ChangeExtension(tempPath, ".wav");
                wavFiles.Add(wavPath);
                
                await ConvertToWavForSpeechRecognitionAsync(tempPath, wavPath, logger);
                logger.LogInformation($"Converted {fileName} to WAV format at {wavPath}");
            }

            // Merge WAV files if more than one
            string finalWavPath;
            if (wavFiles.Count > 1)
            {
                finalWavPath = Path.GetTempFileName() + ".wav";
                await MergeWavFilesAsync(wavFiles, finalWavPath, logger);
                logger.LogInformation($"Merged {wavFiles.Count} WAV files into {finalWavPath}");
            }
            else
            {
                finalWavPath = wavFiles[0];
            }

            // Convert to transcript using Azure AI Speech Services
            return await ConvertSpeechToTextAsync(finalWavPath, language, includeTimestamps, logger);
        }
        finally
        {
            // Clean up all temporary files
            foreach (var (tempPath, _) in tempFiles)
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            
            foreach (var wavPath in wavFiles)
            {
                if (File.Exists(wavPath))
                    File.Delete(wavPath);
            }
        }
    }

    public string ExtractPlainTextFromTranscript(object transcript)
    {
        var sb = new StringBuilder();
        
        // Handle Fast Transcription format
        if (transcript is JsonElement jsonResponse)
        {
            if (jsonResponse.TryGetProperty("results", out var results))
            {
                foreach (var result in results.EnumerateArray())
                {
                    if (result.TryGetProperty("text", out var text))
                    {
                        sb.AppendLine(text.GetString());
                    }
                }
                return sb.ToString().Trim();
            }
        }
        
        // Handle regular transcription format (List<object>)
        var resultsList = transcript as List<object>;
        if (resultsList == null || resultsList.Count == 0)
        {
            // Try to handle as dynamic object with results property
            var dynamicObj = transcript as dynamic;
            if (dynamicObj?.results != null)
            {
                foreach (var result in dynamicObj.results)
                {
                    if (result.text != null)
                    {
                        sb.AppendLine(result.text.ToString());
                    }
                }
                return sb.ToString().Trim();
            }
            
            return string.Empty;
        }
        
        foreach (var result in resultsList)
        {
            if (result is JsonElement jsonElement)
            {
                // Try to extract text from different possible formats
                if (jsonElement.TryGetProperty("text", out var textElement))
                {
                    sb.AppendLine(textElement.GetString());
                }
                else if (jsonElement.TryGetProperty("DisplayText", out var displayTextElement))
                {
                    sb.AppendLine(displayTextElement.GetString());
                }
            }
            else
            {
                // Handle dynamic objects
                var resultObj = result as dynamic;
                if (resultObj != null)
                {
                    if (resultObj.text != null)
                    {
                        sb.AppendLine(resultObj.text.ToString());
                    }
                }
            }
        }
        
        return sb.ToString().Trim();
    }

    private async Task ConvertToWavForSpeechRecognitionAsync(string inputPath, string wavPath, ILogger logger)
    {
        await Task.Run(() =>
        {
            try
            {
                // Try to read the file using Media Foundation (for various formats)
                using var reader = new MediaFoundationReader(inputPath);
                
                // Create WAV writer with the desired format for speech recognition
                // Use PCM 16kHz, mono as this is optimal for speech recognition
                var desiredFormat = new WaveFormat(16000, 16, 1);
                
                // Use resampler if needed                
                if (reader.WaveFormat.SampleRate != desiredFormat.SampleRate || 
                    reader.WaveFormat.Channels != desiredFormat.Channels)
                {
                    using var resampler = new MediaFoundationResampler(reader, desiredFormat);
                    WaveFileWriter.CreateWaveFile(wavPath, resampler);
                }
                else
                {
                    WaveFileWriter.CreateWaveFile(wavPath, reader);
                }
            }
            catch (Exception ex)
            {
                // Fallback to standard AudioFileReader
                logger.LogWarning(ex, "MediaFoundationReader failed, falling back to AudioFileReader");
                
                using var reader = new AudioFileReader(inputPath);
                
                // Create WAV writer with the desired format for speech recognition
                var desiredFormat = new WaveFormat(16000, 16, 1);
                
                // Use resampler if needed
                if (reader.WaveFormat.SampleRate != desiredFormat.SampleRate || 
                    reader.WaveFormat.Channels != desiredFormat.Channels)
                {
                    using var resampler = new WaveFormatConversionStream(desiredFormat, reader);
                    WaveFileWriter.CreateWaveFile(wavPath, resampler);
                }
                else
                {
                    WaveFileWriter.CreateWaveFile(wavPath, reader);
                }
            }
        });
    }

    private async Task MergeWavFilesAsync(List<string> wavFiles, string outputPath, ILogger logger)
    {
        await Task.Run(() =>
        {
            // Create a list to store all readers
            List<AudioFileReader> readers = new List<AudioFileReader>();
            
            try
            {
                // Open all input files
                foreach (var filePath in wavFiles)
                {
                    readers.Add(new AudioFileReader(filePath));
                }
                
                // Get format from the first file
                var format = readers[0].WaveFormat;
                
                // Create mixer to combine audio
                var mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(readers.Select(r => r.ToSampleProvider()));
                
                // Set mixing behavior to sum all inputs instead of averaging
                mixer.MixerInputEnded += (sender, args) => { };
                
                // Write to output WAV file
                WaveFileWriter.CreateWaveFile16(outputPath, mixer);
            }
            finally
            {
                // Dispose all readers
                foreach (var reader in readers)
                {
                    reader?.Dispose();
                }
            }
        });
    }

    private async Task<object> ConvertSpeechToTextAsync(string wavPath, string language, bool includeTimestamps, ILogger logger)
    {
        // Get configuration from environment
        var speechKey = Environment.GetEnvironmentVariable("SpeechServiceKey");
        var speechRegion = Environment.GetEnvironmentVariable("SpeechServiceRegion");
        
        // Validate configuration
        if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
        {
            throw new InvalidOperationException("Speech service configuration is missing. Please set SpeechServiceKey and SpeechServiceRegion in application settings.");
        }

        // Configure speech recognition with the specified language
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = language;
        
        // Enable detailed output with word timing if timestamps are requested
        if (includeTimestamps)
        {
            speechConfig.OutputFormat = OutputFormat.Detailed;
        }
        
        // Configure audio input from file
        using var audioInput = AudioConfig.FromWavFileInput(wavPath);
        
        // Create speech recognizer
        using var recognizer = new SpeechRecognizer(speechConfig, audioInput);
        
        // Implement continuous recognition with pause detection
        var transcriptionResults = new List<object>();
        var taskCompletionSource = new TaskCompletionSource<int>();
        
        // Handle recognized speech
        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                logger.LogInformation($"RECOGNIZED: Text={e.Result.Text}");
                
                if (includeTimestamps && !string.IsNullOrEmpty(e.Result.Text))
                {
                    // Get detailed output with word timing
                    var detailedResult = JsonSerializer.Deserialize<dynamic>(e.Result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult));
                    transcriptionResults.Add(detailedResult);
                }
                else if (!string.IsNullOrEmpty(e.Result.Text))
                {
                    // Just store the recognized text
                    transcriptionResults.Add(new { text = e.Result.Text });
                }
            }
        };
        
        // Handle session stopped event
        recognizer.SessionStopped += (s, e) =>
        {
            logger.LogInformation("Speech recognition session stopped.");
            taskCompletionSource.SetResult(0);
        };
        
        // Start continuous recognition
        await recognizer.StartContinuousRecognitionAsync();
        
        // Wait for recognition to complete
        await taskCompletionSource.Task;
        
        // Stop recognition
        await recognizer.StopContinuousRecognitionAsync();
        
        return transcriptionResults;
    }

    public async Task<object> FastTranscribeAudioAsync(IFormFile audioFile, string language, ILogger logger)
    {
        // Get configuration from environment
        var speechKey = Environment.GetEnvironmentVariable("SpeechServiceKey");
        var speechRegion = Environment.GetEnvironmentVariable("SpeechServiceRegion");
        
        // Validate configuration
        if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
        {
            throw new InvalidOperationException("Speech service configuration is missing. Please set SpeechServiceKey and SpeechServiceRegion in application settings.");
        }

        string tempFilePath = Path.GetTempFileName();
        
        try
        {
            // Save uploaded file to temporary location
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(fileStream);
            }
            
            logger.LogInformation($"Saved file: {audioFile.FileName}, Size: {audioFile.Length} bytes to {tempFilePath}");

            // Create HTTP client for Fast Transcription API
            using var httpClient = new HttpClient();
            
            // Set up authentication header
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechKey);
            
            // Fast Transcription endpoint URL for direct transcription
            var endpoint = $"https://{speechRegion}.api.cognitive.microsoft.com/speechtotext/transcriptions:transcribe?api-version=2024-05-15-preview";
            
            // Create multipart form content
            using var formContent = new MultipartFormDataContent();
            
            // Add configuration as a single definition JSON
            var definitionObj = new
            {
                locales = new[] { language },
                profanityFilterMode = "Masked",
                addWordLevelTimestamps = false,
                diarizationSettings = new
                {
                    minSpeakers = 1,
                    maxSpeakers = 4
                }
            };
            
            var definitionJson = JsonSerializer.Serialize(definitionObj);
            formContent.Add(new StringContent(definitionJson, Encoding.UTF8, "application/json"), "definition");
            
            // Add audio file
            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(tempFilePath));
            
            // Determine content type based on file extension
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".wav" => "audio/wav",
                ".mp3" => "audio/mpeg",
                ".mp4" => "audio/mp4", 
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                ".flac" => "audio/flac",
                ".ogg" => "audio/ogg",
                _ => "audio/wav"
            };
            
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            formContent.Add(fileContent, "audio", audioFile.FileName);
            
            logger.LogInformation($"Sending Fast Transcription request to {endpoint}");
            
            // Send POST request for immediate transcription
            var response = await httpClient.PostAsync(endpoint, formContent);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError($"Fast Transcription request failed: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Fast Transcription request failed: {response.StatusCode} - {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogInformation($"Fast Transcription response received, length: {responseContent.Length}");
            
            // Parse the response
            var transcriptionResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            // Extract the transcription results in a format similar to the regular transcription
            var results = new List<object>();
            
            if (transcriptionResponse.TryGetProperty("combinedPhrases", out var combinedPhrases))
            {
                foreach (var phrase in combinedPhrases.EnumerateArray())
                {
                    if (phrase.TryGetProperty("text", out var text))
                    {
                        var result = new
                        {
                            text = text.GetString(),
                            confidence = phrase.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 1.0,
                            offset = phrase.TryGetProperty("offsetInTicks", out var offset) ? offset.GetInt64() : 0,
                            duration = phrase.TryGetProperty("durationInTicks", out var duration) ? duration.GetInt64() : 0
                        };
                        results.Add(result);
                    }
                }
            }
            
            // If no combined phrases, try to extract from phrases array
            if (results.Count == 0 && transcriptionResponse.TryGetProperty("phrases", out var phrases))
            {
                foreach (var phrase in phrases.EnumerateArray())
                {
                    if (phrase.TryGetProperty("nBest", out var nBest) && nBest.GetArrayLength() > 0)
                    {
                        var bestResult = nBest[0];
                        if (bestResult.TryGetProperty("display", out var display))
                        {
                            var result = new
                            {
                                text = display.GetString(),
                                confidence = bestResult.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 1.0,
                                offset = phrase.TryGetProperty("offsetInTicks", out var offset) ? offset.GetInt64() : 0,
                                duration = phrase.TryGetProperty("durationInTicks", out var duration) ? duration.GetInt64() : 0
                            };
                            results.Add(result);
                        }
                    }
                }
            }
            
            // Create response in the same format as regular transcription
            var finalResult = new
            {
                success = true,
                results = results,
                originalFileName = audioFile.FileName,
                language = language,
                service = "FastTranscription",
                totalDuration = transcriptionResponse.TryGetProperty("durationInTicks", out var totalDur) ? totalDur.GetInt64() : 0
            };
            
            return finalResult;
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
