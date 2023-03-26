using pandemic.perftest;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            RunSamples(TimeSpan.FromSeconds(4));
        }
        if (args is ["bench"])
        {
            Benchmarks.RunAll();
        }

        return 0;
    }

    private static void RunSamples(TimeSpan forThisLong)
    {
        Console.WriteLine("Running random games...");
        var stats = GameRunner.RunRandomGames(new RunConfig
        {
            TotalRunTime = forThisLong,
            Rng = new Random(1234)
        });
        Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");

        Console.WriteLine("Running greedy games...");
        stats = GameRunner.RunGreedyGames(new RunConfig
        {
            TotalRunTime = forThisLong,
            Rng = new Random(1234)
        });
        Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");

        Console.WriteLine("Running greedy BFS...");
        stats = GameRunner.RunGreedyBfsGames(new RunConfig
        {
            TotalRunTime = forThisLong,
            Rng = new Random(1234)
        });
        Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");
    }
}
