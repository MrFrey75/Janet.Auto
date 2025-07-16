using NAudio.Wave;
using AudioWorkstation.Core.Models;
using NAudio.Dsp; // Add this for BiQuadFilter

namespace AudioWorkstation.Core.SampleProviders;

public class EqualizerSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly EqualizerSettings _settings;
    private readonly BiQuadFilter[] _filters;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public EqualizerSampleProvider(ISampleProvider source, EqualizerSettings settings)
    {
        _source = source;
        _settings = settings;
        _filters = new BiQuadFilter[7];
        
        InitializeFilters();
        _settings.PropertyChanged += (s, e) => UpdateFilters();
    }

    private void InitializeFilters()
    {
        var frequencies = new float[] { 60f, 170f, 310f, 600f, 1000f, 3000f, 6000f };
        var bands = _settings.GetBands();
        
        for (int i = 0; i < _filters.Length; i++)
        {
            _filters[i] = BiQuadFilter.PeakingEQ(_source.WaveFormat.SampleRate, frequencies[i], 1.0f, bands[i]);
        }
    }

    private void UpdateFilters()
    {
        var frequencies = new float[] { 60f, 170f, 310f, 600f, 1000f, 3000f, 6000f };
        var bands = _settings.GetBands();
        
        for (int i = 0; i < _filters.Length; i++)
        {
            _filters[i] = BiQuadFilter.PeakingEQ(_source.WaveFormat.SampleRate, frequencies[i], 1.0f, bands[i]);
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        
        for (int i = 0; i < samplesRead; i++)
        {
            float sample = buffer[offset + i];
            foreach (var filter in _filters)
            {
                sample = filter.Transform(sample);
            }
            buffer[offset + i] = sample;
        }
        
        return samplesRead;
    }
}