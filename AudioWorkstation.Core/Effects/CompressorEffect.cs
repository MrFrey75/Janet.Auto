using NAudio.Wave;

namespace AudioWorkstation.Core.Effects;

public class CompressorEffect : ISoundEffect
{
    public string Name => "Compressor";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; private set; }

    public CompressorEffect()
    {
        Parameters = new Dictionary<string, object>
        {
            ["Threshold"] = 0.7f,
            ["Ratio"] = 4.0f,
            ["Attack"] = 0.003f,
            ["Release"] = 0.1f,
            ["MakeupGain"] = 1.0f
        };
    }

    public ISampleProvider Process(ISampleProvider input)
    {
        return new CompressorSampleProvider(input, this);
    }

    public void SetParameter(string name, object value)
    {
        if (Parameters.ContainsKey(name))
            Parameters[name] = value;
    }

    public T GetParameter<T>(string name)
    {
        if (Parameters.TryGetValue(name, out var value) && value is T typedValue)
            return typedValue;
        return default(T);
    }
}

public class CompressorSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly CompressorEffect _effect;
    private float _envelope;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public CompressorSampleProvider(ISampleProvider source, CompressorEffect effect)
    {
        _source = source;
        _effect = effect;
        _envelope = 0f;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        
        if (!_effect.IsEnabled)
            return samplesRead;

        var threshold = _effect.GetParameter<float>("Threshold");
        var ratio = _effect.GetParameter<float>("Ratio");
        var attack = _effect.GetParameter<float>("Attack");
        var release = _effect.GetParameter<float>("Release");
        var makeupGain = _effect.GetParameter<float>("MakeupGain");

        for (int i = 0; i < samplesRead; i++)
        {
            float input = buffer[offset + i];
            float inputLevel = Math.Abs(input);

            // Envelope follower
            if (inputLevel > _envelope)
                _envelope += (inputLevel - _envelope) * attack;
            else
                _envelope += (inputLevel - _envelope) * release;

            // Compression calculation
            float gain = 1f;
            if (_envelope > threshold)
            {
                float excess = _envelope - threshold;
                float compressedExcess = excess / ratio;
                gain = (threshold + compressedExcess) / _envelope;
            }

            buffer[offset + i] = input * gain * makeupGain;
        }

        return samplesRead;
    }
}