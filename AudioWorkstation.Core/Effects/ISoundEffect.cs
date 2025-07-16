using NAudio.Wave;

namespace AudioWorkstation.Core.Effects;

public interface ISoundEffect
{
    string Name { get; }
    bool IsEnabled { get; set; }
    Dictionary<string, object> Parameters { get; }
    ISampleProvider Process(ISampleProvider input);
    void SetParameter(string name, object value);
    T GetParameter<T>(string name);
}