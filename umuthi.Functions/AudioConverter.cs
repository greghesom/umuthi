using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Lame;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure.Identity;
using System.Text;
using System.Text.Json;

namespace umuthi.Functions;

public partial class AudioConverter
{
    private readonly ILogger<AudioConverter> _logger;

    public AudioConverter(ILogger<AudioConverter> logger)
    {
        _logger = logger;
    }    [Function("ConvertWavToMp3")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertWavToMp3([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // Validate API key
            if (!umuthi.Functions.ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }
            
            _logger.LogInformation("WAV to MP3 conversion function triggered.");

            // Check if a file was uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No file uploaded. Please upload a WAV file.");
            }

            var uploadedFile = req.Form.Files[0];

            // Validate file extension
            if (!uploadedFile.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return new BadRequestObjectResult("Invalid file type. Please upload a WAV file.");
            }

            // Validate file size (limit to 50MB)
            if (uploadedFile.Length > 50 * 1024 * 1024)
            {
                return new BadRequestObjectResult("File too large. Maximum size is 50MB.");
            }

            _logger.LogInformation($"Processing file: {uploadedFile.FileName}, Size: {uploadedFile.Length} bytes");

            // Create temporary files for processing
            var tempWavPath = Path.GetTempFileName();
            var tempMp3Path = Path.ChangeExtension(tempWavPath, ".mp3");

            try
            {
                // Save uploaded WAV file to temporary location
                using (var fileStream = new FileStream(tempWavPath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }                // Convert WAV to MP3
                await BufferedAudioHelpers.ConvertWavToMp3WithBufferingAsync(tempWavPath, tempMp3Path, _logger);

                // Read the converted MP3 file
                var mp3Data = await File.ReadAllBytesAsync(tempMp3Path);

                // Generate output filename
                var outputFileName = Path.ChangeExtension(uploadedFile.FileName, ".mp3");

                _logger.LogInformation($"Conversion completed. Output file: {outputFileName}, Size: {mp3Data.Length} bytes");

                // Return the MP3 file
                return new FileContentResult(mp3Data, "audio/mpeg")
                {
                    FileDownloadName = outputFileName
                };
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempWavPath))
                    File.Delete(tempWavPath);
                if (File.Exists(tempMp3Path))
                    File.Delete(tempMp3Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during WAV to MP3 conversion");
            return new StatusCodeResult(500);
        }
    }    private async Task ConvertWavToMp3Async(string wavFilePath, string mp3FilePath)
    {
        await Task.Run(() =>
        {
            using var reader = new AudioFileReader(wavFilePath);
            
            // Configure MP3 encoding settings
            var mp3Writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.STANDARD);
            
            try
            {
                // Use a buffered approach to reduce memory usage
                byte[] buffer = new byte[4096]; // 4KB buffer size
                int bytesRead;
                
                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    mp3Writer.Write(buffer, 0, bytesRead);
                }
            }
            finally
            {
                mp3Writer?.Dispose();
            }
        });
    }    [Function("ConvertMpegToMp3")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertMpegToMp3([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // Validate API key
            if (!umuthi.Functions.ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }
            
            _logger.LogInformation("MPEG to MP3 conversion function triggered.");

            // Check if a file was uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No file uploaded. Please upload an MPEG file.");
            }

            var uploadedFile = req.Form.Files[0];

            // Validate file extension (support common MPEG formats)
            var validExtensions = new[] { ".mpeg", ".mpg", ".mp4", ".m4a", ".aac" };
            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            
            if (!validExtensions.Contains(fileExtension))
            {
                return new BadRequestObjectResult($"Invalid file type. Please upload an MPEG file. Supported formats: {string.Join(", ", validExtensions)}");
            }

            // Validate file size (limit to 50MB)
            if (uploadedFile.Length > 50 * 1024 * 1024)
            {
                return new BadRequestObjectResult("File too large. Maximum size is 50MB.");
            }

            _logger.LogInformation($"Processing file: {uploadedFile.FileName}, Size: {uploadedFile.Length} bytes");

            // Create temporary files for processing
            var tempInputPath = Path.GetTempFileName();
            var tempMp3Path = Path.ChangeExtension(tempInputPath, ".mp3");

            try
            {
                // Save uploaded file to temporary location
                using (var fileStream = new FileStream(tempInputPath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }                // Convert MPEG to MP3
                await BufferedAudioHelpers.ConvertMpegToMp3WithBufferingAsync(tempInputPath, tempMp3Path, _logger);

                // Read the converted MP3 file
                var mp3Data = await File.ReadAllBytesAsync(tempMp3Path);

                // Generate output filename
                var outputFileName = Path.ChangeExtension(uploadedFile.FileName, ".mp3");

                _logger.LogInformation($"Conversion completed. Output file: {outputFileName}, Size: {mp3Data.Length} bytes");

                // Return the MP3 file
                return new FileContentResult(mp3Data, "audio/mpeg")
                {
                    FileDownloadName = outputFileName
                };
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempInputPath))
                    File.Delete(tempInputPath);
                if (File.Exists(tempMp3Path))
                    File.Delete(tempMp3Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during MPEG to MP3 conversion");
            return new StatusCodeResult(500);
        }
    }

    private async Task ConvertMpegToMp3Async(string inputFilePath, string mp3FilePath)
    {
        await Task.Run(() =>
        {
            try
            {
                // Try to read the input file using MediaFoundationReader for MPEG files
                using var reader = new MediaFoundationReader(inputFilePath);
                
                // Configure MP3 encoding settings
                var mp3Writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.STANDARD);
                
                try
                {
                    reader.CopyTo(mp3Writer);
                }
                finally
                {
                    mp3Writer?.Dispose();
                }
            }
            catch (Exception)
            {
                // Fallback: try with AudioFileReader for other supported formats
                using var reader = new AudioFileReader(inputFilePath);
                
                var mp3Writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.STANDARD);
                
                try
                {
                    reader.CopyTo(mp3Writer);
                }
                finally
                {
                    mp3Writer?.Dispose();
                }
            }
        });
    }    [Function("ConvertMpegToTranscript")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertMpegToTranscript([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // Validate API key
            if (!umuthi.Functions.ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }
            
            _logger.LogInformation("MPEG to Transcript conversion function triggered.");

            // Check if files were uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No files uploaded. Please upload one or more MPEG audio files.");
            }// Get user preferences from query parameters
            string language = req.Query["language"].ToString();
            if (string.IsNullOrEmpty(language))
            {
                language = "en-US"; // Default to English (US)
            }

            bool includeTimestamps = req.Query.ContainsKey("timestamps") && 
                                     req.Query["timestamps"].ToString().ToLower() == "true";

            _logger.LogInformation($"Processing {req.Form.Files.Count} audio files with language: {language}, timestamps: {includeTimestamps}");

            // Validate file extensions and sizes
            var validExtensions = new[] { ".mpeg", ".mpg", ".mp4", ".m4a", ".aac", ".mp3", ".wav" };
            List<(string TempPath, string FileName)> tempFiles = new();

            long totalSize = 0;
            foreach (var file in req.Form.Files)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!validExtensions.Contains(fileExtension))
                {
                    return new BadRequestObjectResult($"Invalid file type: {file.FileName}. Supported formats: {string.Join(", ", validExtensions)}");
                }

                totalSize += file.Length;
                if (totalSize > 100 * 1024 * 1024) // 100MB total limit
                {
                    return new BadRequestObjectResult("Total file size too large. Maximum size is 100MB.");
                }

                var tempPath = Path.GetTempFileName();
                tempFiles.Add((tempPath, file.FileName));
                
                // Save uploaded file to temp location
                using (var fileStream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                
                _logger.LogInformation($"Saved file: {file.FileName}, Size: {file.Length} bytes to {tempPath}");
            }

            // Convert files to WAV format for speech recognition
            List<string> wavFiles = new();
            
            try
            {
                foreach (var (tempPath, fileName) in tempFiles)
                {
                    var wavPath = Path.ChangeExtension(tempPath, ".wav");
                    wavFiles.Add(wavPath);
                    
                    await ConvertToWavForSpeechRecognitionAsync(tempPath, wavPath);
                    _logger.LogInformation($"Converted {fileName} to WAV format at {wavPath}");
                }

                // Merge WAV files if more than one
                string finalWavPath;
                if (wavFiles.Count > 1)
                {
                    finalWavPath = Path.GetTempFileName() + ".wav";
                    await MergeWavFilesAsync(wavFiles, finalWavPath);
                    _logger.LogInformation($"Merged {wavFiles.Count} WAV files into {finalWavPath}");
                }
                else
                {
                    finalWavPath = wavFiles[0];
                }

                // Convert to transcript using Azure AI Speech Services
                var transcript = await ConvertSpeechToTextAsync(finalWavPath, language, includeTimestamps);
                
                // Format response based on user preferences
                if (includeTimestamps)
                {
                    return new OkObjectResult(transcript);
                }
                else
                {
                    // Extract just the text from the transcript
                    var plainText = ExtractPlainTextFromTranscript(transcript);
                    return new ContentResult
                    {
                        Content = plainText,
                        ContentType = "text/plain",
                        StatusCode = 200
                    };
                }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during MPEG to Transcript conversion");
            return new StatusCodeResult(500);
        }
    }    private async Task ConvertToWavForSpeechRecognitionAsync(string inputPath, string wavPath)
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
                _logger.LogWarning(ex, "MediaFoundationReader failed, falling back to AudioFileReader");
                
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
    }    private async Task MergeWavFilesAsync(List<string> wavFiles, string outputPath)
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
                var mixer = new MixingSampleProvider(readers.Select(r => r.ToSampleProvider()));
                
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

    private async Task<object> ConvertSpeechToTextAsync(string wavPath, string language, bool includeTimestamps)
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
                _logger.LogInformation($"RECOGNIZED: Text={e.Result.Text}");
                
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
            _logger.LogInformation("Speech recognition session stopped.");
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

    private string ExtractPlainTextFromTranscript(object transcript)
    {
        var resultsList = transcript as List<object>;
        if (resultsList == null || resultsList.Count == 0)
        {
            return string.Empty;
        }
        
        var sb = new StringBuilder();
        
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

    // Update the GetSupportedFormats method to include the new functionality
    /*[Function("GetSupportedFormats")]
    public IActionResult GetSupportedFormats([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var supportedFormats = new
        {
            input = new[] { "WAV", "MPEG", "MPG", "MP4", "M4A", "AAC", "MP3" },
            output = new[] { "MP3", "TEXT" },
            maxFileSize = "50MB for conversion, 100MB for transcription",
            functions = new[]
            {
                new
                {
                    endpoint = "/api/ConvertWavToMp3",
                    method = "POST",
                    contentType = "multipart/form-data",
                    description = "Upload a WAV file to convert it to MP3 format",
                    supportedInputs = new[] { "WAV" }
                },
                new
                {
                    endpoint = "/api/ConvertMpegToMp3",
                    method = "POST",
                    contentType = "multipart/form-data",
                    description = "Upload an MPEG file to convert it to MP3 format",
                    supportedInputs = new[] { "MPEG", "MPG", "MP4", "M4A", "AAC" }
                },
                new
                {                    endpoint = "/api/ConvertMpegToTranscript",
                    method = "POST",
                    contentType = "multipart/form-data",
                    description = "Upload one or more audio files to convert them to a transcript",
                    supportedInputs = new[] { "MPEG", "MPG", "MP4", "M4A", "AAC", "MP3", "WAV" },
                    queryParameters = new object[]
                    {
                        new { name = "language", description = "Speech recognition language (e.g., en-US, fr-FR)", required = false, defaultValue = "en-US" },
                        new { name = "timestamps", description = "Include word timestamps in output (true/false)", required = false, defaultValue = "false" }
                    }
                }
            }
        };

        return new OkObjectResult(supportedFormats);
    }*/
}

