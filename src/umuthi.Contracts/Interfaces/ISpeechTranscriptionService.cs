using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace umuthi.Contracts.Interfaces;

/// <summary>
/// Interface for speech transcription services
/// </summary>
public interface ISpeechTranscriptionService
{
    /// <summary>
    /// Transcribes multiple audio files to text
    /// </summary>
    /// <param name="audioFiles">Collection of audio files to transcribe</param>
    /// <param name="language">Language code for transcription (e.g., "en-US")</param>
    /// <param name="includeTimestamps">Whether to include word-level timestamps</param>
    /// <param name="logger">Logger for tracking the transcription process</param>
    /// <returns>Transcription results as an object</returns>
    Task<object> TranscribeAudioFilesAsync(
        IEnumerable<IFormFile> audioFiles, 
        string language, 
        bool includeTimestamps, 
        ILogger logger);

    /// <summary>
    /// Extracts plain text from transcript results
    /// </summary>
    /// <param name="transcript">Transcript object containing recognition results</param>
    /// <returns>Plain text representation of the transcript</returns>
    string ExtractPlainTextFromTranscript(object transcript);

    /// <summary>
    /// Fast transcription using Azure Speech Services Fast Transcription API
    /// </summary>
    /// <param name="audioFile">Audio file to transcribe</param>
    /// <param name="language">Language code for transcription (e.g., "en-US")</param>
    /// <param name="logger">Logger for tracking the transcription process</param>
    /// <returns>Fast transcription results</returns>
    Task<object> FastTranscribeAudioAsync(
        IFormFile audioFile, 
        string language, 
        ILogger logger);
}