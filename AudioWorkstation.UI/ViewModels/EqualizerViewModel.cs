using System;
using ReactiveUI;
using AudioWorkstation.Core.Models;
using System.Reactive;

namespace AudioWorkstation.UI.ViewModels;

public class EqualizerViewModel : ViewModelBase
{
    private EqualizerSettings _settings;
    private bool _isEnabled = true;
    private string _title;

    public EqualizerViewModel(string title)
    {
        _title = title;
        _settings = new EqualizerSettings();
        
        ResetCommand = ReactiveCommand.Create(Reset);
        
        // Subscribe to settings changes
        _settings.PropertyChanged += (s, e) => 
        {
            BandChanged?.Invoke(this, 0f); // Signal that a band changed
        };
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public EqualizerSettings Settings
    {
        get => _settings;
        set => this.RaiseAndSetIfChanged(ref _settings, value);
    }

    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    public event EventHandler<float>? BandChanged;

    private void Reset()
    {
        _settings.Reset();
    }
}