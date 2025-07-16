using System;
using System.Linq;
using System.Threading;
using AudioWorkstation.Core.Services;
using Avalonia.Threading;

namespace AudioWorkstation.UI.Services;

public interface IAudioVisualizationService
{
    float[] SpectrumData { get; }
    float[] WaveformData { get; }
    float MasterLevel { get; }
    float[] ChannelLevels { get; }
    
    event EventHandler<float[]>? SpectrumDataUpdated;
    event EventHandler<float>? MasterLevelUpdated;
    event EventHandler<float[]>? ChannelLevelsUpdated;
    
    void StartVisualization();
    void StopVisualization();
    void UpdateAudioData(float[] samples);
}

public class AudioVisualizationService : IAudioVisualizationService, IDisposable
{
    private readonly SpectrumAnalyzer _spectrumAnalyzer;
    private readonly Timer _updateTimer;
    private readonly object _lockObject = new();
    private bool _disposed;
    private float[] _currentSamples = new float[1024];

    public float[] SpectrumData { get; private set; } = new float[512];
    public float[] WaveformData { get; private set; } = new float[1024];
    public float MasterLevel { get; private set; }
    public float[] ChannelLevels { get; private set; } = new float[8];

    public event EventHandler<float[]>? SpectrumDataUpdated;
    public event EventHandler<float>? MasterLevelUpdated;
    public event EventHandler<float[]>? ChannelLevelsUpdated;

    public AudioVisualizationService()
    {
        _spectrumAnalyzer = new SpectrumAnalyzer(1024);
        _updateTimer = new Timer(UpdateVisualization, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void StartVisualization()
    {
        _updateTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(50)); // 20 FPS
    }

    public void StopVisualization()
    {
        _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void UpdateAudioData(float[] samples)
    {
        lock (_lockObject)
        {
            if (samples.Length >= _currentSamples.Length)
            {
                Array.Copy(samples, _currentSamples, _currentSamples.Length);
            }
            else
            {
                Array.Copy(samples, 0, _currentSamples, 0, samples.Length);
                Array.Clear(_currentSamples, samples.Length, _currentSamples.Length - samples.Length);
            }

            // Update master level
            MasterLevel = samples.Length > 0 ? samples.Max(Math.Abs) : 0f;
        }
    }

    private void UpdateVisualization(object? state)
    {
        try
        {
            lock (_lockObject)
            {
                // Update spectrum data
                SpectrumData = _spectrumAnalyzer.GetSpectrum(_currentSamples);
                
                // Update waveform data (downsample if needed)
                WaveformData = _currentSamples.Take(1024).ToArray();
            }

            // Raise events on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                SpectrumDataUpdated?.Invoke(this, SpectrumData);
                MasterLevelUpdated?.Invoke(this, MasterLevel);
                ChannelLevelsUpdated?.Invoke(this, ChannelLevels);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating visualization: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _updateTimer?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}