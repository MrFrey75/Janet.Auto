using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioWorkstation.Core.Effects;

public class ReverbEffect : ISoundEffect
{
    public string Name => "Reverb";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; private set; }

    public ReverbEffect()
    {
        Parameters = new Dictionary<string, object>
        {
            ["RoomSize"] = 0.5f,
            ["Damping"] = 0.5f,
            ["WetLevel"] = 0.3f,
            ["DryLevel"] = 0.7f
        };
    }

    public ISampleProvider Process(ISampleProvider input)
    {
        // Simple reverb implementation using delay lines
        return new ReverbSampleProvider(input, this);
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

public class ReverbSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly ReverbEffect _effect;
    private readonly DelayLine[] _delayLines;
    private readonly float[] _buffer;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public ReverbSampleProvider(ISampleProvider source, ReverbEffect effect)
    {
        _source = source;
        _effect = effect;
        _buffer = new float[1024];
        
        // Initialize delay lines for reverb
        var delayTimes = new int[] { 1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617 };
        _delayLines = new DelayLine[delayTimes.Length];
        
        for (int i = 0; i < delayTimes.Length; i++)
        {
            _delayLines[i] = new DelayLine(delayTimes[i]);
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        
        if (!_effect.IsEnabled)
            return samplesRead;

        var roomSize = _effect.GetParameter<float>("RoomSize");
        var damping = _effect.GetParameter<float>("Damping");
        var wetLevel = _effect.GetParameter<float>("WetLevel");
        var dryLevel = _effect.GetParameter<float>("DryLevel");

        for (int i = 0; i < samplesRead; i++)
        {
            float input = buffer[offset + i];
            float output = 0f;

            // Process through delay lines
            foreach (var delayLine in _delayLines)
            {
                output += delayLine.Process(input, roomSize, damping);
            }

            output /= _delayLines.Length;
            buffer[offset + i] = (input * dryLevel) + (output * wetLevel);
        }

        return samplesRead;
    }

    private class DelayLine
    {
        private readonly float[] _buffer;
        private int _index;
        private float _feedback;

        public DelayLine(int length)
        {
            _buffer = new float[length];
            _index = 0;
            _feedback = 0f;
        }

        public float Process(float input, float roomSize, float damping)
        {
            float output = _buffer[_index];
            _feedback = output * (1f - damping) + _feedback * damping;
            _buffer[_index] = input + _feedback * roomSize;
            
            _index = (_index + 1) % _buffer.Length;
            return output;
        }
    }
}