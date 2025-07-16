namespace AudioWorkstation.Core.Services;

public interface ILoopService
{
    bool IsEnabled { get; set; }
    TimeSpan LoopStart { get; set; }
    TimeSpan LoopEnd { get; set; }
    TimeSpan CurrentPosition { get; set; }
    
    bool ShouldLoop(TimeSpan position);
    TimeSpan GetLoopPosition(TimeSpan position);
    void SetLoopRegion(TimeSpan start, TimeSpan end);
}

public class LoopService : ILoopService
{
    public bool IsEnabled { get; set; }
    public TimeSpan LoopStart { get; set; }
    public TimeSpan LoopEnd { get; set; }
    public TimeSpan CurrentPosition { get; set; }

    public bool ShouldLoop(TimeSpan position)
    {
        return IsEnabled && position >= LoopEnd && LoopEnd > LoopStart;
    }

    public TimeSpan GetLoopPosition(TimeSpan position)
    {
        if (!IsEnabled || LoopEnd <= LoopStart)
            return position;

        if (position >= LoopEnd)
        {
            var loopDuration = LoopEnd - LoopStart;
            var overshoot = position - LoopEnd;
            return LoopStart + TimeSpan.FromTicks(overshoot.Ticks % loopDuration.Ticks);
        }

        return position;
    }

    public void SetLoopRegion(TimeSpan start, TimeSpan end)
    {
        if (start < end)
        {
            LoopStart = start;
            LoopEnd = end;
        }
    }
}