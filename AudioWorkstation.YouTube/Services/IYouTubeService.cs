using AudioWorkstation.Core.Models;
using AudioWorkstation.Core.Services;

namespace AudioWorkstation.YouTube.Services;

public interface IYouTubeService
{
    Task<YouTubeVideo?> GetVideoInfoAsync(string url);
    Task<string?> GetAudioStreamUrlAsync(string videoId);
    Task<Stream?> GetAudioStreamAsync(string streamUrl);
    bool IsValidYouTubeUrl(string url);
    string? ExtractVideoId(string url);
}