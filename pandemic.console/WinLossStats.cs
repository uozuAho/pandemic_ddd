using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Events;
using pandemic.Values;
using utils;

namespace pandemic.console;

public class WinLossStats
{
    public static void PlayGamesAndPrintWinLossStats(ILiveAgent agent, TimeSpan timeLimit)
    {
        PlayGamesAndPrintWinLossStats(agent, timeLimit, () =>
        {
            var random = new Random();
            var numPlayers = random.Choice(new[] {2, 3, 4});
            return new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = random.Choice(numPlayers, Enum.GetValues<Role>()).ToArray(),
                CommandGenerator = new SensibleCommandGenerator()
            };
        });
    }

    private static void PlayGamesAndPrintWinLossStats(
        ILiveAgent agent,
        TimeSpan timeLimit,
        Func<NewGameOptions> createOptions)
    {
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Playing games for {timeLimit.TotalSeconds} seconds...");
        var winningOptions = new List<NewGameOptions>();
        var losingOptions = new List<NewGameOptions>();

        while (stopwatch.Elapsed < timeLimit)
        {
            var options = createOptions();
            var (game, _) = PandemicGame.CreateNewGame(options);
            var (endState, events) = PlayGame(game, agent);
            if (endState.IsWon) winningOptions.Add(options);
            else losingOptions.Add(options);
        }

        var wins = winningOptions.Count;
        var losses = losingOptions.Count;
        var winsByNumPlayers = winningOptions.GroupBy(o => o.Roles.Count).ToDictionary(g => g.Key, g => g.Count());

        Console.WriteLine($"{wins + losses} games played. {wins} wins, {losses} losses");
        Console.WriteLine($"Wins by number of players: {string.Join(", ", winsByNumPlayers.Select(kvp => $"{kvp.Key} players: {kvp.Value}"))}");
    }

    private static (PandemicGame, IEnumerable<IEvent>) PlayGame(PandemicGame game, ILiveAgent agent)
    {
        var numActions = 0;
        var events = new List<IEvent>();

        for (; numActions < 1000 && !game.IsOver; numActions++)
        {
            if (numActions == 999) throw new InvalidOperationException("didn't expect this many turns");
            var command = agent.NextCommand(game);
            game = game.Do(command, events);
        }

        return (game, events);
    }
}
