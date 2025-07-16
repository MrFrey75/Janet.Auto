using NAudio.Dsp;
// Remove: using System.Numerics; // This causes the ambiguous reference

namespace AudioWorkstation.Core.Services;

public class SpectrumAnalyzer
{
    private readonly int _fftLength;
    private readonly NAudio.Dsp.Complex[] _fftBuffer; // Explicitly use NAudio.Dsp.Complex
    private readonly float[] _window;
    private readonly float[] _spectrumData;

    public SpectrumAnalyzer(int fftLength = 1024)
    {
        _fftLength = fftLength;
        _fftBuffer = new NAudio.Dsp.Complex[fftLength];
        _window = new float[fftLength];
        _spectrumData = new float[fftLength / 2];
        
        // Create Hanning window
        for (int i = 0; i < fftLength; i++)
        {
            _window[i] = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (fftLength - 1)));
        }
    }

    public float[] GetSpectrum(float[] audioData)
    {
        if (audioData.Length < _fftLength)
            return _spectrumData;

        // Apply window and convert to complex
        for (int i = 0; i < _fftLength; i++)
        {
            _fftBuffer[i] = new NAudio.Dsp.Complex
            {
                X = audioData[i] * _window[i], // Real part
                Y = 0 // Imaginary part
            };
        }

        // Perform FFT
        FastFourierTransform.FFT(true, (int)Math.Log2(_fftLength), _fftBuffer);

        // Calculate magnitude spectrum
        for (int i = 0; i < _fftLength / 2; i++)
        {
            var magnitude = Math.Sqrt(_fftBuffer[i].X * _fftBuffer[i].X + 
                                      _fftBuffer[i].Y * _fftBuffer[i].Y);
            _spectrumData[i] = (float)(20 * Math.Log10(magnitude + 1e-10)); // Convert to dB
        }

        return _spectrumData;
    }

    public float[] GetFrequencyBins(float sampleRate)
    {
        var bins = new float[_fftLength / 2];
        for (int i = 0; i < bins.Length; i++)
        {
            bins[i] = i * sampleRate / _fftLength;
        }
        return bins;
    }
}