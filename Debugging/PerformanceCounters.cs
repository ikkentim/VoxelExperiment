namespace MyGame.Debugging;

public static class PerformanceCounters
{
    public static readonly PerformanceCounter Drawing = new();
    public static readonly PerformanceCounter Update = new();
    public static readonly PerformanceCounter Cumulative = new();
}