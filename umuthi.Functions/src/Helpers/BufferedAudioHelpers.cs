using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Lame;
using System.IO;
using System.Threading.Tasks;
using System;

namespace umuthi.Functions.Helpers;

public class BufferedAudioHelpers
{
    public static async Task ConvertWavToMp3WithBufferingAsync(string wavFilePath, string mp3FilePath, ILogger logger)
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
                long totalBytesProcessed = 0;
                
                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    mp3Writer.Write(buffer, 0, bytesRead);
                    totalBytesProcessed += bytesRead;
                    
                    // Log progress for large files (every 5MB)
                    if (totalBytesProcessed % (5 * 1024 * 1024) < buffer.Length)
                    {
                        logger.LogInformation($"Processed {totalBytesProcessed / (1024 * 1024)}MB of audio data");
                    }
                }
                
                logger.LogInformation($"Completed processing {totalBytesProcessed / (1024 * 1024)}MB of audio data");
            }
            finally
            {
                mp3Writer?.Dispose();
            }
        });
    }
    
    public static async Task ConvertMpegToMp3WithBufferingAsync(string inputFilePath, string mp3FilePath, ILogger logger)
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
                    // Use a buffered approach to reduce memory usage
                    byte[] buffer = new byte[4096]; // 4KB buffer size
                    int bytesRead;
                    long totalBytesProcessed = 0;
                    
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        mp3Writer.Write(buffer, 0, bytesRead);
                        totalBytesProcessed += bytesRead;
                        
                        // Log progress for large files (every 5MB)
                        if (totalBytesProcessed % (5 * 1024 * 1024) < buffer.Length)
                        {
                            logger.LogInformation($"Processed {totalBytesProcessed / (1024 * 1024)}MB of audio data");
                        }
                    }
                    
                    logger.LogInformation($"Completed processing {totalBytesProcessed / (1024 * 1024)}MB of audio data");
                }
                finally
                {
                    mp3Writer?.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MediaFoundationReader failed, falling back to AudioFileReader");
                
                // Fallback: try with AudioFileReader for other supported formats
                using var reader = new AudioFileReader(inputFilePath);
                
                var mp3Writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.STANDARD);
                
                try
                {
                    // Use a buffered approach to reduce memory usage
                    byte[] buffer = new byte[4096]; // 4KB buffer size
                    int bytesRead;
                    long totalBytesProcessed = 0;
                    
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        mp3Writer.Write(buffer, 0, bytesRead);
                        totalBytesProcessed += bytesRead;
                        
                        // Log progress for large files (every 5MB)
                        if (totalBytesProcessed % (5 * 1024 * 1024) < buffer.Length)
                        {
                            logger.LogInformation($"Processed {totalBytesProcessed / (1024 * 1024)}MB of audio data");
                        }
                    }
                    
                    logger.LogInformation($"Completed processing {totalBytesProcessed / (1024 * 1024)}MB of audio data");
                }
                finally
                {
                    mp3Writer?.Dispose();
                }
            }
        });
    }
}
