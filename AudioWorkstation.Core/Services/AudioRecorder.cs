using NAudio.Wave;

namespace AudioWorkstation.Core.Services;

public interface IAudioRecorder
{
    bool IsRecording { get; }
    void StartRecording(string filePath);
    void StopRecording();
    event EventHandler<float> LevelChanged;
}

public class AudioRecorder : IAudioRecorder, IDisposable
{
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _writer;
    private bool _disposed;

    public bool IsRecording { get; private set; }

    public event EventHandler<float>? LevelChanged;

    public void StartRecording(string filePath)
    {
        if (IsRecording)
            StopRecording();

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 16, 2)
        };

        _writer = new WaveFileWriter(filePath, _waveIn.WaveFormat);

        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        _waveIn.StartRecording();
        IsRecording = true;
    }

    public void StopRecording()
    {
        if (!IsRecording)
            return;

        _waveIn?.StopRecording();
        IsRecording = false;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _writer?.Write(e.Buffer, 0, e.BytesRecorded);

        // Calculate level for meter
        float level = 0f;
        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            short sample = BitConverter.ToInt16(e.Buffer, i);
            float sampleValue = sample / 32768f;
            level = Math.Max(level, Math.Abs(sampleValue));
        }

        LevelChanged?.Invoke(this, level);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _writer?.Dispose();
        _writer = null;
        _waveIn?.Dispose();
        _waveIn = null;
        IsRecording = false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopRecording();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}