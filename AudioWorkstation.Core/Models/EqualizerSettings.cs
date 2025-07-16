using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioWorkstation.Core.Models;

public class EqualizerSettings : INotifyPropertyChanged
{
    private float _band60Hz = 0f;
    private float _band170Hz = 0f;
    private float _band310Hz = 0f;
    private float _band600Hz = 0f;
    private float _band1kHz = 0f;
    private float _band3kHz = 0f;
    private float _band6kHz = 0f;

    public float Band60Hz
    {
        get => _band60Hz;
        set { _band60Hz = ClampValue(value); OnPropertyChanged(); }
    }

    public float Band170Hz
    {
        get => _band170Hz;
        set { _band170Hz = ClampValue(value); OnPropertyChanged(); }
    }

    public float Band310Hz
    {
        get => _band310Hz;
        set { _band310Hz = ClampValue(value); OnPropertyChanged(); }
    }

    public float Band600Hz
    {
        get => _band600Hz;
        set { _band600Hz = ClampValue(value); OnPropertyChanged(); }
    }

    public float Band1kHz
    {
        get => _band1kHz;
        set { _band1kHz = ClampValue(value); OnPropertyChanged(); }
    }

    public float Band3kHz
    {
        get => _band3kHz;
        set { _band3kHz = ClampValue(value); OnPropertyChanged(); }
    }

    public float Band6kHz
    {
        get => _band6kHz;
        set { _band6kHz = ClampValue(value); OnPropertyChanged(); }
    }

    private static float ClampValue(float value) => Math.Max(-20f, Math.Min(20f, value));

    public void Reset()
    {
        Band60Hz = 0f;
        Band170Hz = 0f;
        Band310Hz = 0f;
        Band600Hz = 0f;
        Band1kHz = 0f;
        Band3kHz = 0f;
        Band6kHz = 0f;
    }

    public float[] GetBands()
    {
        return new float[] { Band60Hz, Band170Hz, Band310Hz, Band600Hz, Band1kHz, Band3kHz, Band6kHz };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}