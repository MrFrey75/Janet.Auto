using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioWorkstation.Core.Services;

public interface IMetronomeService
{
    bool IsEnabled { get; set; }
    int Bpm { get; set; }
    int BeatsPerMeasure { get; set; }
    float Volume { get; set; }
    
    void Start();
    void Stop();
    ISampleProvider GetAudioOutput();
}

public class MetronomeService : IMetronomeService, IDisposable
{
    private readonly Timer _timer;
    private readonly SignalGenerator _clickGenerator;
    private readonly VolumeSampleProvider _volumeProvider;
    private bool _disposed;
    private int _currentBeat;

    public bool IsEnabled { get; set; }
    public int Bpm { get; set; } = 120;
    public int BeatsPerMeasure { get; set; } = 4;
    public float Volume { get; set; } = 0.5f;

    public MetronomeService()
    {
        _clickGenerator = new SignalGenerator(44100, 1);
        _volumeProvider = new VolumeSampleProvider(_clickGenerator);
        
        _timer = new Timer(OnTimerTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        if (IsEnabled)
        {
            var interval = TimeSpan.FromMilliseconds(60000.0 / Bpm);
            _timer.Change(TimeSpan.Zero, interval);
        }
    }

    public void Stop()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _currentBeat = 0;
    }

    public ISampleProvider GetAudioOutput()
    {
        _volumeProvider.Volume = Volume;
        return _volumeProvider;
    }

    private void OnTimerTick(object? state)
    {
        if (!IsEnabled) return;

        _currentBeat = (_currentBeat + 1) % BeatsPerMeasure;
        
        // Generate click sound
        var frequency = _currentBeat == 0 ? 800f : 400f; // Accent first beat
        var duration = TimeSpan.FromMilliseconds(100);
        
        Task.Run(() =>
        {
            _clickGenerator.Frequency = frequency;
            _clickGenerator.Type = SignalGeneratorType.Sin;
            
            // Brief click sound
            Thread.Sleep(50);
            _clickGenerator.Frequency = 0;
        });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _timer?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}