using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using AudioWorkstation.Core.Models;
using AudioWorkstation.Core.Effects;
using AudioWorkstation.Core.SampleProviders;

namespace AudioWorkstation.Core.Services;

public class AudioService : IAudioService
{
    private readonly ConcurrentDictionary<string, IAudioChannel> _channels;
    private readonly Dictionary<string, List<string>> _duckingTriggers;
    private IWavePlayer? _waveOut;
    private MixingSampleProvider? _mixer;
    private readonly WaveFormat _waveFormat;
    private bool _disposed;

    public float MasterVolume { get; set; } = 1.0f;
    public bool IsMasterMuted { get; set; } = false;
    public DuckingSettings DuckingSettings { get; set; }
    public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;

    public event EventHandler<string>? ChannelAdded;
    public event EventHandler<string>? ChannelRemoved;
    public event EventHandler<float>? VolumeChanged;

    public AudioService()
    {
        _channels = new ConcurrentDictionary<string, IAudioChannel>();
        _duckingTriggers = new Dictionary<string, List<string>>();
        _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        _mixer = new MixingSampleProvider(_waveFormat);
        DuckingSettings = new DuckingSettings();
        
        InitializeAudioOutput();
    }

    private void InitializeAudioOutput()
    {
        try
        {
            // Use WaveOutEvent for all platforms - it's more reliable
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_mixer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize audio output: {ex.Message}", ex);
        }
    }

    public IAudioChannel CreateChannel(string name)
    {
        var channel = new AudioChannel(name);
        _channels[channel.Id] = channel;
        ChannelAdded?.Invoke(this, channel.Id);
        return channel;
    }

    public void RemoveChannel(string channelId)
    {
        if (_channels.TryRemove(channelId, out var channel))
        {
            // Remove from ducking triggers
            var keysToRemove = _duckingTriggers.Where(kvp => kvp.Value.Contains(channelId)).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToRemove)
            {
                _duckingTriggers[key].Remove(channelId);
                if (!_duckingTriggers[key].Any())
                    _duckingTriggers.Remove(key);
            }
            
            ChannelRemoved?.Invoke(this, channelId);
        }
    }

    public IAudioChannel? GetChannel(string channelId)
    {
        return _channels.TryGetValue(channelId, out var channel) ? channel : null;
    }

    public IEnumerable<IAudioChannel> GetAllChannels()
    {
        return _channels.Values;
    }

    public void SetDuckingTrigger(string triggerChannelId, params string[] targetChannelIds)
    {
        _duckingTriggers[triggerChannelId] = targetChannelIds.ToList();
    }

    public void RemoveDuckingTrigger(string triggerChannelId)
    {
        _duckingTriggers.Remove(triggerChannelId);
    }

    public void StartPlayback()
    {
        if (_waveOut?.PlaybackState != PlaybackState.Playing)
        {
            UpdateMixer();
            _waveOut?.Play();
        }
    }

    public void StopPlayback()
    {
        _waveOut?.Stop();
    }

    public void PausePlayback()
    {
        _waveOut?.Pause();
    }

    public void SetOutputDevice(int deviceIndex)
    {
        _waveOut?.Stop();
        _waveOut?.Dispose();
        
        // Use WaveOutEvent for device switching - more reliable than DirectSoundOut
        _waveOut = new WaveOutEvent { DeviceNumber = deviceIndex };
        _waveOut.Init(_mixer);
    }

    public List<string> GetAvailableOutputDevices()
    {
        var devices = new List<string>();
    
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Use DirectSoundOut for Windows - correct API
                var directSoundDevices = DirectSoundOut.Devices;
                int index = 0;
                foreach (var device in directSoundDevices)
                {
                    devices.Add(index.ToString() + ": " + device.Description);
                    index++;
                }
            }
            else
            {
                // For non-Windows platforms, use MMDevice API or fallback
                devices.Add("0: Default Audio Device");
            }
        }
        catch (Exception ex)
        {
            // Ultimate fallback - add default device
            devices.Clear();
            devices.Add("0: Default Audio Device");
            Console.WriteLine("Warning: Could not enumerate audio devices: " + ex.Message);
        }
    
        // Ensure we always have at least one device
        if (devices.Count == 0)
        {
            devices.Add("0: Default Audio Device");
        }
    
        return devices;
    }

    private void UpdateMixer()
    {
        _mixer?.RemoveAllMixerInputs();
        
        foreach (var channel in _channels.Values.OfType<AudioChannel>())
        {
            var processedAudio = channel.GetProcessedAudio();
            if (processedAudio != null)
            {
                _mixer?.AddMixerInput(processedAudio);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}