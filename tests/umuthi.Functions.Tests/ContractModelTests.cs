using Xunit;
using umuthi.Contracts.Models;

namespace umuthi.Functions.Tests;

public class ContractModelTests
{
    [Fact]
    public void AudioConversionResult_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var result = new AudioConversionResult();
        
        // Assert
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(string.Empty, result.FileName);
        Assert.Equal("audio/mpeg", result.ContentType);
        Assert.Equal(0, result.SizeInBytes);
    }
    
    [Fact]
    public void OperationTypes_ShouldHaveExpectedConstants()
    {
        // Assert
        Assert.Equal("AudioConversion", OperationTypes.AudioConversion);
        Assert.Equal("SpeechTranscription", OperationTypes.SpeechTranscription);
        Assert.Equal("ApiInfo", OperationTypes.ApiInfo);
        Assert.Equal("UsageAnalytics", OperationTypes.UsageAnalytics);
    }
}