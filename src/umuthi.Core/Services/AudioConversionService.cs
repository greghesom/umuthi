using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Lame;
using System.IO;
using System.Threading.Tasks;
using System;
using umuthi.Contracts.Models;
using umuthi.Contracts.Interfaces;
using umuthi.Shared.Helpers;

namespace umuthi.Core.Services;

/// <summary>
/// Service for converting audio files between different formats
/// </summary>
public class AudioConversionService : IAudioConversionService
{
    public async Task<AudioConversionResult> ConvertWavToMp3Async(IFormFile wavFile, ILogger logger)
    {
        // Create temporary files for processing
        var tempWavPath = Path.GetTempFileName();
        var tempMp3Path = Path.ChangeExtension(tempWavPath, ".mp3");

        try
        {
            // Save uploaded WAV file to temporary location
            using (var fileStream = new FileStream(tempWavPath, FileMode.Create))
            {
                await wavFile.CopyToAsync(fileStream);
            }

            // Convert WAV to MP3 using buffered helpers
            await BufferedAudioHelpers.ConvertWavToMp3WithBufferingAsync(tempWavPath, tempMp3Path, logger);

            // Read the converted MP3 file
            var mp3Data = await File.ReadAllBytesAsync(tempMp3Path);

            // Generate output filename
            var outputFileName = Path.ChangeExtension(wavFile.FileName, ".mp3");

            return new AudioConversionResult
            {
                Data = mp3Data,
                FileName = outputFileName,
                ContentType = "audio/mpeg"
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

    public async Task<AudioConversionResult> ConvertMpegToMp3Async(IFormFile mpegFile, ILogger logger)
    {
        // Create temporary files for processing
        var tempInputPath = Path.GetTempFileName();
        var tempMp3Path = Path.ChangeExtension(tempInputPath, ".mp3");

        try
        {
            // Save uploaded file to temporary location
            using (var fileStream = new FileStream(tempInputPath, FileMode.Create))
            {
                await mpegFile.CopyToAsync(fileStream);
            }

            // Convert MPEG to MP3 using buffered helpers
            await BufferedAudioHelpers.ConvertMpegToMp3WithBufferingAsync(tempInputPath, tempMp3Path, logger);

            // Read the converted MP3 file
            var mp3Data = await File.ReadAllBytesAsync(tempMp3Path);

            // Generate output filename
            var outputFileName = Path.ChangeExtension(mpegFile.FileName, ".mp3");

            return new AudioConversionResult
            {
                Data = mp3Data,
                FileName = outputFileName,
                ContentType = "audio/mpeg"
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
}
