namespace pandemic.perftest;

using agents;
using agents.GreedyBfs;
using Aggregates.Game;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using Commands;
using Values;

public static class Benchmarks
{
    public static void RunAll()
    {
        _ = BenchmarkRunner.Run<LiveAgentBenchmarks>();

        // omitting for now. Takes more time to run, and will likely
        // improve as greedy live agent performance improves.
        // BenchmarkRunner.Run<GreedyBfsBenchmarks>();
    }
}

public class LiveAgentBenchmarks
{
    // [Benchmark]
    // public void PlayOneRandomAgentGame()
    // {
    //     var agent = new RandomAgent();
    //     var game = BenchmarkUtils.NewGame();
    //     while (!game.IsOver)
    //     {
    //         var command = agent.NextCommand(game);
    //         (game, _) = game.Do(command);
    //     }
    // }

    [Benchmark]
    // I think benchmark methods can't be static (todo: check)
#pragma warning disable CA1822
    public void PlayOneGreedyAgentGame()
#pragma warning restore CA1822
    {
        var agent = new GreedyAgent();
        var game = BenchmarkUtils.NewGame();
        while (!game.IsOver)
        {
            var command = agent.NextCommand(game);
            (game, _) = game.Do(command);
        }
    }
}

/// <summary>
/// Greedy BFS perf doesn't have a steady state. The longer it runs,
/// the more game states it will have stored in memory. Also, it's
/// unlikely to finish even one game, whereas the simpler agents can
/// play many games.
///
/// Thus we need a different benchmarking strategy.
/// </summary>
[SimpleJob(RunStrategy.Monitoring, iterationCount: 50, invocationCount: 100)]
public class GreedyBfsBenchmarks
{
    private PandemicGame? _greedyBfsGame;
    private GreedyBestFirstSearch? _greedyBfsSearch;

    public GreedyBfsBenchmarks()
    {
        ResetGreedyBfsGame();
    }

    [Benchmark]
    public void GreedyBfsTakeOneStep()
    {
        _ = _greedyBfsSearch!.Step() ?? throw new InvalidOperationException("game end not handled");
    }

    private void ResetGreedyBfsGame()
    {
        _greedyBfsGame = BenchmarkUtils.NewGame();
        _greedyBfsSearch = new GreedyBestFirstSearch(_greedyBfsGame);
    }
}

internal static class BenchmarkUtils
{
    public static PandemicGame NewGame()
    {
        var options = new NewGameOptions
        {
            Roles = new[] { Role.Medic, Role.Scientist },
            Difficulty = Difficulty.Introductory,
            CommandGenerator = new SensibleCommandGenerator(),
            Rng = new Random(1234),
        };

        var (game, _) = PandemicGame.CreateNewGame(options);

        return game with
        {
            SelfConsistencyCheckingEnabled = false,
        };
    }
}
