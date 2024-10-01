#nullable enable

using System;
using System.Threading.Tasks;

public class Timer
{
    public event Action? TimerElapsed;

    public bool IsTimerElapsed { get; private set; } = true;

    public void StartTimerSeconds(float seconds)
    {
        IsTimerElapsed = false;
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            IsTimerElapsed = true;
            TimerElapsed?.Invoke();
        });
    }
}
