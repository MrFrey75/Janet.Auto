using System;
using ReactiveUI;
using AudioWorkstation.Core.Services;
using AudioWorkstation.Core.Models;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace AudioWorkstation.UI.ViewModels;

public class Mp3PlayerViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private IAudioChannel? _audioChannel;
    private Mp3Track? _currentTrack;
    private string _trackInfo = "No track loaded";
    private string _timeLabel = "00:00";
    private string _durationLabel = "00:00";
    private float _progress = 0f;
    private float _volume = 70f;
    private bool _isPlaying = false;
    private bool _isPaused = false;

    public Mp3PlayerViewModel(IAudioService audioService)
    {
        _audioService = audioService;
        Playlist = new ObservableCollection<Mp3Track>();
        
        // Commands
        LoadFileCommand = ReactiveCommand.CreateFromTask(LoadFile);
        PlayCommand = ReactiveCommand.Create(Play, this.WhenAnyValue(x => x.CanPlay));
        PauseCommand = ReactiveCommand.Create(Pause, this.WhenAnyValue(x => x.CanPause));
        StopCommand = ReactiveCommand.Create(Stop, this.WhenAnyValue(x => x.CanStop));
        AddToPlaylistCommand = ReactiveCommand.Create(AddToPlaylist);
        RemoveFromPlaylistCommand = ReactiveCommand.Create(RemoveFromPlaylist);
    }

    public string TrackInfo
    {
        get => _trackInfo;
        set => this.RaiseAndSetIfChanged(ref _trackInfo, value);
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

    public bool CanPlay => _currentTrack != null && !IsPlaying;
    public bool CanPause => _currentTrack != null && IsPlaying;
    public bool CanStop => _currentTrack != null && (IsPlaying || IsPaused);

    public ObservableCollection<Mp3Track> Playlist { get; }
    public Mp3Track? SelectedPlaylistTrack { get; set; }

    public ReactiveCommand<Unit, Unit> LoadFileCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<Unit, Unit> AddToPlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveFromPlaylistCommand { get; }

    private async Task LoadFile()
    {
        // This would need to be implemented with proper file dialog
        // For now, simulate loading a file
        await Task.Delay(100);
        
        var track = new Mp3Track
        {
            FilePath = @"C:\Music\sample.mp3",
            Title = "Sample Track",
            Artist = "Sample Artist",
            Duration = TimeSpan.FromMinutes(3.5)
        };
        
        LoadTrack(track);
    }

    private void LoadTrack(Mp3Track track)
    {
        _currentTrack = track;
        TrackInfo = track.DisplayName;
        DurationLabel = FormatTime(track.Duration);
        
        // Create audio channel if needed
        _audioChannel ??= _audioService.CreateChannel("MP3 Player");
        
        try
        {
            _audioChannel.SetAudioSource(track.FilePath);
            this.RaisePropertyChanged(nameof(CanPlay));
        }
        catch (Exception ex)
        {
            TrackInfo = $"Error loading file: {ex.Message}";
        }
    }

    private void Play()
    {
        if (_currentTrack != null && _audioChannel != null)
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
        if (_currentTrack != null)
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
        if (_currentTrack != null)
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

    private void AddToPlaylist()
    {
        if (_currentTrack != null && !Playlist.Contains(_currentTrack))
        {
            Playlist.Add(_currentTrack);
        }
    }

    private void RemoveFromPlaylist()
    {
        if (SelectedPlaylistTrack != null)
        {
            Playlist.Remove(SelectedPlaylistTrack);
        }
    }

    private static string FormatTime(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
    }
}