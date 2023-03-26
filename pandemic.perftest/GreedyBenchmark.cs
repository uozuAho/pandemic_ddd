using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using pandemic.agents;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Values;

namespace pandemic.perftest;

public class GreedyBenchmark
{
    private readonly GreedyAgent _agent;

    public GreedyBenchmark()
    {
        _agent = new GreedyAgent();
    }

    public static void Run()
    {
        BenchmarkRunner.Run<GreedyBenchmark>();
    }

    [Benchmark]
    public void RunGame()
    {
        var game = NewGame();
        while (!game.IsOver)
        {
            var command = _agent.NextCommand(game);
            (game, _) = game.Do(command);
        }
    }

    private PandemicGame NewGame()
    {
        var options = new NewGameOptions
        {
            Roles = new[] { Role.Medic, Role.Scientist },
            Difficulty = Difficulty.Introductory,
            CommandGenerator = new SensibleCommandGenerator(),
            Rng = new Random(1234)
        };

        var (game, _) = PandemicGame.CreateNewGame(options);

        return game with { SelfConsistencyCheckingEnabled = false };
    }
}
