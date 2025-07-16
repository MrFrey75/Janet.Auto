using System;
using ReactiveUI;
using AudioWorkstation.Core.Services;
using AudioWorkstation.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioWorkstation.UI.ViewModels;

public class YouTubeStreamViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private IAudioChannel? _audioChannel;
    private YouTubeVideo? _currentVideo;
    private string _url = string.Empty;
    private string _videoInfo = "No video loaded";
    private string _timeLabel = "00:00";
    private string _durationLabel = "00:00";
    private float _progress = 0f;
    private float _volume = 70f;
    private bool _isPlaying = false;
    private bool _isPaused = false;
    private bool _isLoading = false;

    public YouTubeStreamViewModel(IAudioService audioService)
    {
        _audioService = audioService;
        History = new ObservableCollection<YouTubeVideo>();
        
        // Commands
        LoadVideoCommand = ReactiveCommand.CreateFromTask(LoadVideo, this.WhenAnyValue(x => x.CanLoad));
        PlayCommand = ReactiveCommand.Create(Play, this.WhenAnyValue(x => x.CanPlay));
        PauseCommand = ReactiveCommand.Create(Pause, this.WhenAnyValue(x => x.CanPause));
        StopCommand = ReactiveCommand.Create(Stop, this.WhenAnyValue(x => x.CanStop));
        ClearHistoryCommand = ReactiveCommand.Create(ClearHistory);
    }

    public string Url
    {
        get => _url;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _url, value);
            this.RaisePropertyChanged(nameof(CanLoad));
        }
    }

    public string VideoInfo
    {
        get => _videoInfo;
        set => this.RaiseAndSetIfChanged(ref _videoInfo, value);
    }

    public string TimeLabel
    {
        get => _timeLabel;
        set => this.RaiseAndSetIfChanged(ref _timeLabel, value);
    }

    public string DurationLabel
    {
        get => _durationLabel;
        set => this.RaiseAndSetIfChanged(ref _durationLabel, value);
    }

    public float Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public float Volume
    {
        get => _volume;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _volume, value);
            if (_audioChannel != null)
                _audioChannel.Volume = value / 100f;
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    public bool IsPaused
    {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public bool CanLoad => !string.IsNullOrEmpty(Url) && IsValidYouTubeUrl(Url) && !IsLoading;
    public bool CanPlay => _currentVideo != null && !IsPlaying && !IsLoading;
    public bool CanPause => _currentVideo != null && IsPlaying;
    public bool CanStop => _currentVideo != null && (IsPlaying || IsPaused);

    public ObservableCollection<YouTubeVideo> History { get; }
    public YouTubeVideo? SelectedHistoryVideo { get; set; }

    public ReactiveCommand<Unit, Unit> LoadVideoCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearHistoryCommand { get; }

    private async Task LoadVideo()
    {
        if (string.IsNullOrEmpty(Url) || !IsValidYouTubeUrl(Url))
            return;

        IsLoading = true;
        VideoInfo = "Loading video...";

        try
        {
            // Simulate loading video info (in real implementation, use YouTubeExplode)
            await Task.Delay(1000);
            
            var video = new YouTubeVideo
            {
                Url = Url,
                Title = "Sample YouTube Video",
                Channel = "Sample Channel",
                Duration = TimeSpan.FromMinutes(4.2),
                ThumbnailUrl = "https://img.youtube.com/vi/sample/maxresdefault.jpg"
            };

            _currentVideo = video;
            VideoInfo = video.DisplayName;
            DurationLabel = FormatTime(video.Duration);

            // Create audio channel if needed
            _audioChannel ??= _audioService.CreateChannel("YouTube Stream");

            // Add to history
            if (!History.Any(v => v.Url == video.Url))
            {
                History.Add(video);
            }

            this.RaisePropertyChanged(nameof(CanPlay));
        }
        catch (Exception ex)
        {
            VideoInfo = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            this.RaisePropertyChanged(nameof(CanLoad));
        }
    }

    private void Play()
    {
        if (_currentVideo != null)
        {
            _audioService.StartPlayback();
            IsPlaying = true;
            IsPaused = false;

            this.RaisePropertyChanged(nameof(CanPlay));
            this.RaisePropertyChanged(nameof(CanPause));
            this.RaisePropertyChanged(nameof(CanStop));
        }
    }

    private void Pause()
    {
        if (_currentVideo != null)
        {
            _audioService.PausePlayback();
            IsPlaying = false;
            IsPaused = true;

            this.RaisePropertyChanged(nameof(CanPlay));
            this.RaisePropertyChanged(nameof(CanPause));
        }
    }

    private void Stop()
    {
        if (_currentVideo != null)
        {
            _audioService.StopPlayback();
            IsPlaying = false;
            IsPaused = false;
            Progress = 0;
            TimeLabel = "00:00";

            this.RaisePropertyChanged(nameof(CanPlay));
            this.RaisePropertyChanged(nameof(CanPause));
            this.RaisePropertyChanged(nameof(CanStop));
        }
    }

    private void ClearHistory()
    {
        History.Clear();
    }

    private static bool IsValidYouTubeUrl(string url)
    {
        var youtubeRegex = new Regex(@"^(https?\:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/.+");
        return youtubeRegex.IsMatch(url);
    }

    private static string FormatTime(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
    }
}