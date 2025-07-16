using AudioWorkstation.Core.Services;
using AudioWorkstation.Core.Models;
using Xunit;

namespace AudioWorkstation.Core.Tests;

public class AudioServiceTests : IDisposable
{
    private readonly IAudioService _audioService;

    public AudioServiceTests()
    {
        _audioService = new AudioService();
    }

    [Fact]
    public void CreateChannel_ShouldReturnValidChannel()
    {
        // Arrange
        var channelName = "Test Channel";

        // Act
        var channel = _audioService.CreateChannel(channelName);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal(channelName, channel.Name);
        Assert.NotEmpty(channel.Id);
    }

    [Fact]
    public void GetChannel_WithValidId_ShouldReturnChannel()
    {
        // Arrange
        var channel = _audioService.CreateChannel("Test");

        // Act
        var retrievedChannel = _audioService.GetChannel(channel.Id);

        // Assert
        Assert.NotNull(retrievedChannel);
        Assert.Equal(channel.Id, retrievedChannel.Id);
    }

    [Fact]
    public void RemoveChannel_WithValidId_ShouldRemoveChannel()
    {
        // Arrange
        var channel = _audioService.CreateChannel("Test");

        // Act
        _audioService.RemoveChannel(channel.Id);
        var retrievedChannel = _audioService.GetChannel(channel.Id);

        // Assert
        Assert.Null(retrievedChannel);
    }

    [Fact]
    public void MasterVolume_ShouldBeSettable()
    {
        // Arrange
        var expectedVolume = 0.5f;

        // Act
        _audioService.MasterVolume = expectedVolume;

        // Assert
        Assert.Equal(expectedVolume, _audioService.MasterVolume);
    }

    public void Dispose()
    {
        _audioService?.Dispose();
    }
}