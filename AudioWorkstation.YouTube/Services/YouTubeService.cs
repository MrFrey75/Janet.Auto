using AudioWorkstation.Core.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Text.RegularExpressions;
using AudioWorkstation.Core.Services;
using YoutubeExplode.Common;

namespace AudioWorkstation.YouTube.Services;

public class YouTubeService : IYouTubeService
{
    private readonly YoutubeClient _youtubeClient;

    public YouTubeService()
    {
        _youtubeClient = new YoutubeClient();
    }

    public async Task<YouTubeVideo?> GetVideoInfoAsync(string url)
    {
        try
        {
            var videoId = ExtractVideoId(url);
            if (string.IsNullOrEmpty(videoId))
                return null;

            var video = await _youtubeClient.Videos.GetAsync(videoId);
            
            return new YouTubeVideo
            {
                Url = url,
                Title = video.Title,
                Channel = video.Author.ChannelTitle,
                Duration = video.Duration ?? TimeSpan.Zero,
                ThumbnailUrl = video.Thumbnails.GetWithHighestResolution()?.Url ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error getting video info: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetAudioStreamUrlAsync(string videoId)
    {
        try
        {
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            
            return audioStreamInfo?.Url;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting audio stream: {ex.Message}");
            return null;
        }
    }

    public async Task<Stream?> GetAudioStreamAsync(string streamUrl)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(streamUrl);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting audio stream: {ex.Message}");
        }
        
        return null;
    }

    public bool IsValidYouTubeUrl(string url)
    {
        var youtubeRegex = new Regex(@"^(https?\:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/.+");
        return youtubeRegex.IsMatch(url);
    }

    public string? ExtractVideoId(string url)
    {
        var videoIdRegex = new Regex(@"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/\s]{11})");
        var match = videoIdRegex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }
}