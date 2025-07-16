using System.ComponentModel;
using AudioWorkstation.Core.Effects;
using AudioWorkstation.Core.Models;

namespace AudioWorkstation.Core.Services;

public interface IAudioService : IDisposable
{
    // Channel management
    IAudioChannel CreateChannel(string name);
    void RemoveChannel(string channelId);
    IAudioChannel? GetChannel(string channelId);
    IEnumerable<IAudioChannel> GetAllChannels();

    // Global controls
    float MasterVolume { get; set; }
    bool IsMasterMuted { get; set; }

    // Ducking
    DuckingSettings DuckingSettings { get; set; }
    void SetDuckingTrigger(string triggerChannelId, params string[] targetChannelIds);
    void RemoveDuckingTrigger(string triggerChannelId);

    // Playback control
    void StartPlayback();
    void StopPlayback();
    void PausePlayback();
    bool IsPlaying { get; }

    // Audio device management
    void SetOutputDevice(int deviceIndex);
    List<string> GetAvailableOutputDevices();
    
    // Events
    event EventHandler<string>? ChannelAdded;
    event EventHandler<string>? ChannelRemoved;
    event EventHandler<float>? VolumeChanged;
}

public interface IAudioChannel
{
    string Id { get; }
    string Name { get; set; }
    float Volume { get; set; }
    bool IsMuted { get; set; }
    bool IsDucked { get; set; }
    float DuckLevel { get; set; }
    EqualizerSettings Equalizer { get; set; }
    List<ISoundEffect> Effects { get; }
    
    void AddEffect(ISoundEffect effect);
    void RemoveEffect(ISoundEffect effect);
    void Play();
    void Pause();
    void Stop();
    void SetAudioSource(string filePath);
    void SetAudioSource(Stream audioStream);
}