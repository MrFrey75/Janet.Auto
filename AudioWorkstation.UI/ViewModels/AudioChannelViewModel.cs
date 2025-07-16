using System;
using ReactiveUI;
using AudioWorkstation.Core.Services;
using AudioWorkstation.Core.Models;
using System.Reactive;

namespace AudioWorkstation.UI.ViewModels;

public class AudioChannelViewModel : ViewModelBase
{
    private readonly IAudioChannel _channel;
    private float _volume = 1.0f;
    private bool _isMuted = false;
    private bool _isSolo = false;
    private bool _isSelected = false;
    private string _displayName = string.Empty;
    private float _level = 0f; // Add this field

    public AudioChannelViewModel(IAudioChannel channel)
    {
        _channel = channel;
        _displayName = channel.Name;
        _volume = channel.Volume;
        _isMuted = channel.IsMuted;
        
        EqualizerSettings = new EqualizerSettings();
        
        // Commands
        SelectChannelCommand = ReactiveCommand.Create(() => { IsSelected = true; });
        MuteCommand = ReactiveCommand.Create(() => { IsMuted = !IsMuted; });
        SoloCommand = ReactiveCommand.Create(() => { IsSolo = !IsSolo; });
    }

    public IAudioChannel Channel => _channel;
    public string Id => _channel.Id;

    public string DisplayName
    {
        get => _displayName;
        set => this.RaiseAndSetIfChanged(ref _displayName, value);
    }

    public float Volume
    {
        get => _volume;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _volume, Math.Max(0f, Math.Min(1f, value)));
            _channel.Volume = _volume;
            this.RaisePropertyChanged(nameof(VolumeDb));
            this.RaisePropertyChanged(nameof(VolumePercentage));
        }
    }

    public float VolumePercentage
    {
        get => _volume * 100f;
        set => Volume = value / 100f;
    }

    public string VolumeDb => $"{20 * Math.Log10(Math.Max(0.001f, _volume)):F1} dB";

    public bool IsMuted
    {
        get => _isMuted;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _isMuted, value);
            _channel.IsMuted = value;
        }
    }

    public bool IsSolo
    {
        get => _isSolo;
        set => this.RaiseAndSetIfChanged(ref _isSolo, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    // Add the missing Level property
    public float Level
    {
        get => _level;
        set => this.RaiseAndSetIfChanged(ref _level, Math.Max(0f, Math.Min(1f, value)));
    }

    public EqualizerSettings EqualizerSettings { get; set; }

    public ReactiveCommand<Unit, Unit> SelectChannelCommand { get; }
    public ReactiveCommand<Unit, Unit> MuteCommand { get; }
    public ReactiveCommand<Unit, Unit> SoloCommand { get; }

    // Method to update the level from audio processing
    public void UpdateLevel(float newLevel)
    {
        Level = newLevel;
    }
}