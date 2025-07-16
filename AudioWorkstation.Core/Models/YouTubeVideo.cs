using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioWorkstation.Core.Models;

public class YouTubeVideo : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _channel = string.Empty;
    private string _url = string.Empty;
    private string _thumbnailUrl = string.Empty;
    private TimeSpan _duration;
    private bool _isPlaying;
    private bool _isPaused;
    private string _audioStreamUrl = string.Empty;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    public string Channel
    {
        get => _channel;
        set { _channel = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    public string Url
    {
        get => _url;
        set { _url = value; OnPropertyChanged(); }
    }

    public string ThumbnailUrl
    {
        get => _thumbnailUrl;
        set { _thumbnailUrl = value; OnPropertyChanged(); }
    }

    public TimeSpan Duration
    {
        get => _duration;
        set { _duration = value; OnPropertyChanged(); }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set { _isPlaying = value; OnPropertyChanged(); }
    }

    public bool IsPaused
    {
        get => _isPaused;
        set { _isPaused = value; OnPropertyChanged(); }
    }

    public string AudioStreamUrl
    {
        get => _audioStreamUrl;
        set { _audioStreamUrl = value; OnPropertyChanged(); }
    }

    public string DisplayName => !string.IsNullOrEmpty(Channel) && !string.IsNullOrEmpty(Title)
        ? $"{Channel} - {Title}"
        : Title;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}