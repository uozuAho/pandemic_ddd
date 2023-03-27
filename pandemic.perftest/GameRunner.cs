using System.Diagnostics;
using pandemic.agents;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Values;

namespace pandemic.perftest;

public static class GameRunner
{
    public static RunStats RunRandomGames(RunConfig config)
    {
        var agent = new RandomAgent();
        return RunLiveAgentGames(agent, config);
    }

    public static RunStats RunGreedyGames(RunConfig config)
    {
        var agent = new GreedyAgent();
        return RunLiveAgentGames(agent, config);
    }

    public static RunStats RunGreedyBfsGames(RunConfig config)
    {
        var totalTimer = Stopwatch.StartNew();
        var numGames = 0;
        var commandsExecuted = 0;

        while (totalTimer.Elapsed < config.TotalRunTime)
        {
            var game = NewGame(config);
            var search = new GreedyBestFirstSearch(game);

            while (!game.IsOver && totalTimer.Elapsed < config.TotalRunTime)
            {
                var searchNode = search.Step();
                commandsExecuted++;
                if (searchNode == null) break;
                game = searchNode.State;
            }
            numGames++;
        }

        return new RunStats(numGames, commandsExecuted, config.TotalRunTime);
    }

    private static RunStats RunLiveAgentGames(ILiveAgent agent, RunConfig config)
    {
        var totalTimer = Stopwatch.StartNew();
        var numGames = 0;
        var commandsExecuted = 0;

        while (totalTimer.Elapsed < config.TotalRunTime)
        {
            var game = NewGame(config);

            while (!game.IsOver && totalTimer.Elapsed < config.TotalRunTime)
            {
                var command = agent.NextCommand(game);
                (game, _) = game.Do(command);
                commandsExecuted++;
            }
            numGames++;
        }

        return new RunStats(numGames, commandsExecuted, config.TotalRunTime);
    }

    private static PandemicGame NewGame(RunConfig config)
    {
        var options = new NewGameOptions
        {
            Roles = new[] { Role.Medic, Role.Dispatcher },
            Difficulty = Difficulty.Introductory,
            CommandGenerator = new SensibleCommandGenerator(),
            Rng = config.Rng
        };

        var (game, _) = PandemicGame.CreateNewGame(options);

        return game with { SelfConsistencyCheckingEnabled = false };
    }
}
