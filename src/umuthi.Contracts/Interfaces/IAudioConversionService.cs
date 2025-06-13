using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using umuthi.Contracts.Models;

namespace umuthi.Contracts.Interfaces;

/// <summary>
/// Interface for audio conversion services
/// </summary>
public interface IAudioConversionService
{
    /// <summary>
    /// Converts a WAV file to MP3 format
    /// </summary>
    /// <param name="wavFile">The uploaded WAV file</param>
    /// <param name="logger">Logger for tracking the conversion process</param>
    /// <returns>Conversion result containing the MP3 data and filename</returns>
    Task<AudioConversionResult> ConvertWavToMp3Async(IFormFile wavFile, ILogger logger);

    /// <summary>
    /// Converts an MPEG file to MP3 format
    /// </summary>
    /// <param name="mpegFile">The uploaded MPEG file</param>
    /// <param name="logger">Logger for tracking the conversion process</param>
    /// <returns>Conversion result containing the MP3 data and filename</returns>
    Task<AudioConversionResult> ConvertMpegToMp3Async(IFormFile mpegFile, ILogger logger);
}