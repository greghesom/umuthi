using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Xunit;
using umuthi.Core.Services;
using umuthi.Contracts.Interfaces;

namespace umuthi.Core.Tests;

public class AudioConversionServiceTests
{
    [Fact]
    public void AudioConversionService_ShouldImplementInterface()
    {
        // Arrange & Act
        var service = new AudioConversionService();
        
        // Assert
        Assert.IsAssignableFrom<IAudioConversionService>(service);
    }
}