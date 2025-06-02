// Just the GetSupportedFormats method to fix the issue
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace umuthi.Functions
{
    public partial class AudioConverter
    {        [Function("GetSupportedFormats")]
        [ApiKeyAuthentication]
        public IActionResult GetSupportedFormats([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var supportedFormats = new
            {
                input = new[] { "WAV", "MPEG", "MPG", "MP4", "M4A", "AAC", "MP3" },
                output = new[] { "MP3", "TEXT" },
                maxFileSize = "50MB for conversion, 100MB for transcription",
                functions = new object[]
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
                    {
                        endpoint = "/api/ConvertMpegToTranscript",
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
        }
    }
}
