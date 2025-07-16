using NAudio.Wave;

namespace AudioWorkstation.Core.Effects;

public class DistortionEffect : ISoundEffect
{
    public string Name => "Distortion";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; private set; }

    public DistortionEffect()
    {
        Parameters = new Dictionary<string, object>
        {
            ["Drive"] = 0.5f,
            ["Tone"] = 0.5f,
            ["Level"] = 0.8f,
            ["Type"] = "Overdrive" // "Overdrive", "Fuzz", "Distortion"
        };
    }

    public ISampleProvider Process(ISampleProvider input)
    {
        return new DistortionSampleProvider(input, this);
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
        return default(T)!;
    }
}

public class DistortionSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly DistortionEffect _effect;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public DistortionSampleProvider(ISampleProvider source, DistortionEffect effect)
    {
        _source = source;
        _effect = effect;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        
        if (!_effect.IsEnabled)
            return samplesRead;

        var drive = _effect.GetParameter<float>("Drive");
        var tone = _effect.GetParameter<float>("Tone");
        var level = _effect.GetParameter<float>("Level");
        var type = _effect.GetParameter<string>("Type");

        for (int i = 0; i < samplesRead; i++)
        {
            float input = buffer[offset + i];
            float output = input;

            // Apply drive/gain
            output *= (1.0f + drive * 10.0f);

            // Apply distortion based on type
            switch (type)
            {
                case "Overdrive":
                    output = ApplyOverdrive(output, drive);
                    break;
                case "Fuzz":
                    output = ApplyFuzz(output, drive);
                    break;
                case "Distortion":
                    output = ApplyDistortion(output, drive);
                    break;
            }

            // Apply tone control (simple high-cut filter)
            output = ApplyTone(output, tone);

            // Apply output level
            output *= level;

            // Prevent clipping
            output = Math.Max(-1.0f, Math.Min(1.0f, output));

            buffer[offset + i] = output;
        }

        return samplesRead;
    }

    private static float ApplyOverdrive(float input, float drive)
    {
        var threshold = 0.7f - (drive * 0.4f);
        if (Math.Abs(input) > threshold)
        {
            var sign = Math.Sign(input);
            var excess = Math.Abs(input) - threshold;
            return sign * (threshold + excess * 0.3f);
        }
        return input;
    }

    private static float ApplyFuzz(float input, float drive)
    {
        var factor = 1.0f + drive * 5.0f;
        var distorted = input * factor;
        return Math.Sign(distorted) * Math.Min(Math.Abs(distorted), 1.0f);
    }

    private static float ApplyDistortion(float input, float drive)
    {
        var factor = 1.0f + drive * 3.0f;
        return (float)Math.Tanh(input * factor);
    }

    private static float ApplyTone(float input, float tone)
    {
        // Simple tone control - more sophisticated filtering could be added
        return input * (0.5f + tone * 0.5f);
    }
}