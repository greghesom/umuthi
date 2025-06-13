namespace umuthi.Contracts.Models;

/// <summary>
/// Result of an audio conversion operation
/// </summary>
public class AudioConversionResult
{
    /// <summary>
    /// The converted audio data as a byte array
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The suggested filename for the converted audio
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The MIME type of the converted audio
    /// </summary>
    public string ContentType { get; set; } = "audio/mpeg";

    /// <summary>
    /// The size of the converted audio in bytes
    /// </summary>
    public long SizeInBytes => Data.Length;
}