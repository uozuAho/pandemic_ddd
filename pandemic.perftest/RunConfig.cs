namespace pandemic.perftest;

public record RunConfig
{
    public TimeSpan TotalRunTime { get; init; }
    public Random Rng { get; init; } = new Random();
}
