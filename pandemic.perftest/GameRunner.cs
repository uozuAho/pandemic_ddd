using System.Diagnostics;
using pandemic.agents;
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
        return RunGames(agent, totalRunTime);
    }

    public static RunStats RunGreedyGames(TimeSpan totalRunTime)
    {
        var agent = new GreedyAgent();
        return RunGames(agent, totalRunTime);
    }

    private static RunStats RunGames(ILiveAgent agent, TimeSpan totalRunTime)
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
