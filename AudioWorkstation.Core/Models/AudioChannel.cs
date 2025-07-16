using AudioWorkstation.Core.Effects;
using AudioWorkstation.Core.SampleProviders;
using AudioWorkstation.Core.Services;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioWorkstation.Core.Models;

public class AudioChannel : IAudioChannel
{
    private ISampleProvider? _audioSource;
    private VolumeSampleProvider? _volumeProvider;
    private EqualizerSampleProvider? _equalizerProvider;
    private readonly List<ISoundEffect> _effects;

    public string Id { get; private set; }
    public string Name { get; set; }
    public float Volume 
    { 
        get => _volumeProvider?.Volume ?? 1.0f; 
        set => SetVolume(value); 
    }
    public bool IsMuted { get; set; }
    public bool IsDucked { get; set; }
    public float DuckLevel { get; set; } = 0.3f;
    public EqualizerSettings Equalizer { get; set; }
    public List<ISoundEffect> Effects => _effects;

    public AudioChannel(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Equalizer = new EqualizerSettings();
        _effects = new List<ISoundEffect>();
        IsMuted = false;
        IsDucked = false;
    }

    public void AddEffect(ISoundEffect effect)
    {
        _effects.Add(effect);
    }

    public void RemoveEffect(ISoundEffect effect)
    {
        _effects.Remove(effect);
    }

    public void SetAudioSource(string filePath)
    {
        var audioFile = new AudioFileReader(filePath);
        SetAudioSource(audioFile.ToSampleProvider());
    }

    public void SetAudioSource(Stream audioStream)
    {
        var audioFile = new StreamMediaFoundationReader(audioStream);
        SetAudioSource(audioFile);
    }

    public void SetAudioSource(ISampleProvider sampleProvider)
    {
        _audioSource = sampleProvider;
        _volumeProvider = new VolumeSampleProvider(sampleProvider);
        _equalizerProvider = new EqualizerSampleProvider(_volumeProvider, Equalizer);
    }

    public ISampleProvider? GetProcessedAudio()
    {
        if (_audioSource == null) return null;

        ISampleProvider processed = _equalizerProvider ?? _volumeProvider ?? _audioSource;

        foreach (var effect in _effects.Where(e => e.IsEnabled))
        {
            processed = effect.Process(processed);
        }

        return processed;
    }

    private void SetVolume(float value)
    {
        if (_volumeProvider != null)
        {
            float finalVolume = value;
            if (IsMuted) finalVolume = 0f;
            if (IsDucked) finalVolume *= DuckLevel;
            
            _volumeProvider.Volume = Math.Max(0f, Math.Min(1f, finalVolume));
        }
    }

    public void Play() { /* Implementation depends on service */ }
    public void Pause() { /* Implementation depends on service */ }
    public void Stop() { /* Implementation depends on service */ }
}