namespace pandemic.perftest;

public static class Program
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
        // Console.WriteLine("Running random games...");
        // var stats = GameRunner.RunRandomGames(new RunConfig
        // {
        //     TotalRunTime = forThisLong,
        //     Rng = new Random(1234)
        // });
        // PrintStats(stats);

        Console.WriteLine("Running greedy games...");
        var stats = GameRunner.RunGreedyGames(
            new RunConfig { TotalRunTime = forThisLong, Rng = new Random(1234) }
        );
        PrintStats(stats);

        // Console.WriteLine("Running greedy BFS...");
        // stats = GameRunner.RunGreedyBfsGames(new RunConfig
        // {
        //     TotalRunTime = forThisLong,
        //     Rng = new Random(1234)
        // });
        // PrintStats(stats);
    }

    private static void PrintStats(RunStats stats)
    {
        var seconds = stats.TotalRunTime.TotalSeconds;
        var gamesPerSecond = stats.GamesPlayed / stats.TotalRunTime.TotalSeconds;
        var msPerGame = stats.TotalRunTime.TotalMilliseconds / stats.GamesPlayed;
        var commandsPerSecond = stats.CommandsExecuted / stats.TotalRunTime.TotalSeconds;

        Console.Write($"{stats.GamesPlayed} games played in {seconds:F1}s ");
        Console.WriteLine($"({gamesPerSecond:F2} games/sec, {msPerGame:F2}ms/game)");
        Console.WriteLine(
            $"Commands executed: {stats.CommandsExecuted} ({commandsPerSecond:F2}/sec)"
        );
    }
}
