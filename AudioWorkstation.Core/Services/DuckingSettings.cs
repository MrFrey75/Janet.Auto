namespace AudioWorkstation.Core.Services;

public class DuckingSettings
{
    public float DuckLevel { get; set; } = 0.3f;
    public TimeSpan AttackTime { get; set; } = TimeSpan.FromMilliseconds(50);
    public TimeSpan ReleaseTime { get; set; } = TimeSpan.FromMilliseconds(200);
    public float Threshold { get; set; } = 0.1f;
}