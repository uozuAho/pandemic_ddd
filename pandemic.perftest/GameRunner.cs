using System.Diagnostics;
using pandemic.agents;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.test.Utils;
using pandemic.Values;
using utils;

namespace pandemic.perftest;

public static class GameRunner
{
    public static RunStats RunRandomGames(TimeSpan totalRunTime)
    {
        var agent = new RandomAgent();
        return RunLiveAgentGames(agent, totalRunTime);
    }

    public static RunStats RunGreedyGames(TimeSpan totalRunTime)
    {
        var agent = new GreedyAgent();
        return RunLiveAgentGames(agent, totalRunTime);
    }

    public static RunStats RunGreedyBfsGames(TimeSpan totalRunTime)
    {
        var totalTimer = Stopwatch.StartNew();
        var numGames = 0;
        var commandsExecuted = 0;

        while (totalTimer.Elapsed < totalRunTime)
        {
            var game = NewGame();
            var search = new GreedyBestFirstSearch2(game);

            while (!game.IsOver && totalTimer.Elapsed < totalRunTime)
            {
                var searchNode = search.Step();
                commandsExecuted++;
                if (searchNode == null) break;
                game = searchNode.State;
            }
            numGames++;
        }

        return new RunStats(numGames, commandsExecuted);
    }

    private static RunStats RunLiveAgentGames(ILiveAgent agent, TimeSpan totalRunTime)
    {
        var totalTimer = Stopwatch.StartNew();
        var numGames = 0;
        var commandsExecuted = 0;

        while (totalTimer.Elapsed < totalRunTime)
        {
            var game = NewGame();

            while (!game.IsOver && totalTimer.Elapsed < totalRunTime)
            {
                var command = agent.NextCommand(game);
                (game, _) = game.Do(command);
                commandsExecuted++;
            }
            numGames++;
        }

        return new RunStats(numGames, commandsExecuted);
    }

    private static PandemicGame NewGame()
    {
        var options = NewGameOptionsGenerator.RandomOptions() with
        {
            Roles = new[] { Role.Medic, Role.Dispatcher },
            Difficulty = Difficulty.Introductory,
            CommandGenerator = new SensibleCommandGenerator()
        };

        var (game, _) = PandemicGame.CreateNewGame(options);

        return game with { SelfConsistencyCheckingEnabled = false };
    }
}
