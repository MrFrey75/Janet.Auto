using ReactiveUI;
using AudioWorkstation.Core.Services;
using AudioWorkstation.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace AudioWorkstation.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private string _statusMessage = "Ready";
    private float _masterVolume = 80f;
    private bool _isMasterMuted = false;
    private string _selectedAudioDevice = string.Empty;
    private float _masterLevel = 0f;

    public MainWindowViewModel()
    {
        _audioService = new AudioService();
        
        // Initialize collections
        ChannelViewModels = new ObservableCollection<AudioChannelViewModel>();
        AudioDevices = new ObservableCollection<string>();
        
        // Initialize view models
        Mp3PlayerViewModel = new Mp3PlayerViewModel(_audioService);
        YouTubeStreamViewModel = new YouTubeStreamViewModel(_audioService);
        MasterEqualizerViewModel = new EqualizerViewModel("Master EQ");
        ChannelEqualizerViewModel = new EqualizerViewModel("Channel EQ") { IsEnabled = false };
        
        // Initialize commands
        PlayAllCommand = ReactiveCommand.Create(PlayAll);
        StopAllCommand = ReactiveCommand.Create(StopAll);
        AddChannelCommand = ReactiveCommand.Create(AddChannel);
        
        // Load audio devices
        LoadAudioDevices();
        
        // Set up event handlers
        SetupEventHandlers();
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public float MasterVolume
    {
        get => _masterVolume;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _masterVolume, value);
            _audioService.MasterVolume = value / 100f;
            this.RaisePropertyChanged(nameof(MasterVolumeText));
        }
    }

    public string MasterVolumeText => $"{MasterVolume:F0}%";

    public bool IsMasterMuted
    {
        get => _isMasterMuted;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _isMasterMuted, value);
            _audioService.IsMasterMuted = value;
        }
    }

    public string SelectedAudioDevice
    {
        get => _selectedAudioDevice;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _selectedAudioDevice, value);
            SetAudioDevice(value);
        }
    }

    public float MasterLevel
    {
        get => _masterLevel;
        set => this.RaiseAndSetIfChanged(ref _masterLevel, value);
    }

    public ObservableCollection<AudioChannelViewModel> ChannelViewModels { get; }
    public ObservableCollection<string> AudioDevices { get; }

    public Mp3PlayerViewModel Mp3PlayerViewModel { get; }
    public YouTubeStreamViewModel YouTubeStreamViewModel { get; }
    public EqualizerViewModel MasterEqualizerViewModel { get; }
    public EqualizerViewModel ChannelEqualizerViewModel { get; }

    public ReactiveCommand<Unit, Unit> PlayAllCommand { get; }
    public ReactiveCommand<Unit, Unit> StopAllCommand { get; }
    public ReactiveCommand<Unit, Unit> AddChannelCommand { get; }

    private void PlayAll()
    {
        _audioService.StartPlayback();
        StatusMessage = "Playback started";
    }

    private void StopAll()
    {
        _audioService.StopPlayback();
        StatusMessage = "Playback stopped";
    }

    private void AddChannel()
    {
        var channel = _audioService.CreateChannel($"Channel {ChannelViewModels.Count + 1}");
        var viewModel = new AudioChannelViewModel(channel);
        ChannelViewModels.Add(viewModel);
        StatusMessage = $"Added channel: {channel.Name}";
    }

    private void LoadAudioDevices()
    {
        var devices = _audioService.GetAvailableOutputDevices();
        AudioDevices.Clear();
        foreach (var device in devices)
        {
            AudioDevices.Add(device);
        }
        
        if (AudioDevices.Count > 0)
        {
            SelectedAudioDevice = AudioDevices[0];
        }
    }

    private void SetAudioDevice(string deviceName)
    {
        if (!string.IsNullOrEmpty(deviceName))
        {
            var index = AudioDevices.IndexOf(deviceName);
            if (index >= 0)
            {
                _audioService.SetOutputDevice(index);
                StatusMessage = $"Audio device changed to: {deviceName}";
            }
        }
    }

    private void SetupEventHandlers()
    {
        _audioService.ChannelAdded += (s, channelId) =>
        {
            var channel = _audioService.GetChannel(channelId);
            if (channel != null)
            {
                var existing = ChannelViewModels.FirstOrDefault(vm => vm.Id == channelId);
                if (existing == null)
                {
                    var viewModel = new AudioChannelViewModel(channel);
                    ChannelViewModels.Add(viewModel);
                }
            }
        };

        _audioService.ChannelRemoved += (s, channelId) =>
        {
            var viewModel = ChannelViewModels.FirstOrDefault(vm => vm.Id == channelId);
            if (viewModel != null)
            {
                ChannelViewModels.Remove(viewModel);
            }
        };
    }
}