using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioWorkstation.Core.Models;

public class Mp3Track : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _artist = string.Empty;
    private string _filePath = string.Empty;
    private TimeSpan _duration;
    private bool _isPlaying;
    private bool _isPaused;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    public string Artist
    {
        get => _artist;
        set { _artist = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    public string FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
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

    public string DisplayName => !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Artist) 
        ? $"{Artist} - {Title}" 
        : Path.GetFileNameWithoutExtension(FilePath);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}