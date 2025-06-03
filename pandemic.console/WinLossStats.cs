namespace pandemic.console;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using agents;
using Aggregates.Game;
using Commands;
using Events;
using utils;
using Values;

public static class WinLossStats
{
    public static void PlayGamesAndPrintWinLossStats(ILiveAgent agent, TimeSpan timeLimit)
    {
        PlayGamesAndPrintWinLossStats(
            agent,
            timeLimit,
            () =>
            {
                var random = new Random();
                // var numPlayers = random.Choice(new[] {2, 3, 4});
                var numPlayers = 2;
                return new NewGameOptions
                {
                    Difficulty = Difficulty.Introductory,
                    Roles = random.Choice(numPlayers, Enum.GetValues<Role>()).ToArray(),
                    CommandGenerator = new SensibleCommandGenerator(),
                };
            }
        );
    }

    private static void PlayGamesAndPrintWinLossStats(
        ILiveAgent agent,
        TimeSpan timeLimit,
        Func<NewGameOptions> createOptions
    )
    {
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Playing games for {timeLimit.TotalSeconds} seconds...");
        var winningOptions = new List<NewGameOptions>();
        var losingOptions = new List<NewGameOptions>();

        while (stopwatch.Elapsed < timeLimit)
        {
            var options = createOptions();
            var (game, _) = PandemicGame.CreateNewGame(options);
            var (endState, _) = PlayGame(game, agent);
            if (endState.IsWon)
            {
                winningOptions.Add(options);
            }
            else
            {
                losingOptions.Add(options);
            }
        }

        PrintWinLossStats(winningOptions, losingOptions, timeLimit);
    }

    private static void PrintWinLossStats(
        List<NewGameOptions> winningOptions,
        List<NewGameOptions> losingOptions,
        TimeSpan timeLimit
    )
    {
        var teamStats = new Dictionary<string, int[]>();
        var roleStats = new Dictionary<Role, int[]>();

        static string RolesToString(IEnumerable<Role> roles)
        {
            return string.Join(", ", roles.Select(r => r.ToString()[0]).Order()).PadRight(10);
        }

        foreach (var options in winningOptions)
        {
            var roles = RolesToString(options.Roles);
            if (!teamStats.TryGetValue(roles, out var value))
            {
                teamStats[roles] = [1, 0];
            }
            else
            {
                value[0]++;
            }

            foreach (var role in options.Roles)
            {
                if (!roleStats.TryGetValue(role, out var value2))
                {
                    roleStats[role] = [1, 0];
                }
                else
                {
                    value2[0]++;
                }
            }
        }
        foreach (var options in losingOptions)
        {
            var roles = RolesToString(options.Roles);
            if (!teamStats.TryGetValue(roles, out var value))
            {
                teamStats[roles] = [0, 1];
            }
            else
            {
                value[1]++;
            }

            foreach (var role in options.Roles)
            {
                if (!roleStats.TryGetValue(role, out var value2))
                {
                    roleStats[role] = [0, 1];
                }
                else
                {
                    value2[1]++;
                }
            }
        }

        var totalGames = winningOptions.Count + losingOptions.Count;
        var gamesPerSecond = totalGames / timeLimit.TotalSeconds;
        Console.WriteLine(
            $"Played {totalGames} games in {timeLimit.TotalSeconds} seconds ({gamesPerSecond:0.0} games/s)"
        );

        Console.WriteLine();
        Console.WriteLine("Role stats:");
        foreach (
            var stats in roleStats.OrderByDescending(s =>
                s.Value[0] / (s.Value[0] + (double)s.Value[1])
            )
        )
        {
            var (role, winloss) = stats;
            var winPercentage = 100 * winloss[0] / (double)(winloss[0] + winloss[1]);
            Console.WriteLine(
                $"{role}: {winloss[0]} wins, {winloss[1]} losses ({winPercentage:0.0}%)"
            );
        }

        Console.WriteLine();
        Console.WriteLine("Team stats:");
        foreach (
            var stats in teamStats
                .OrderBy(s => s.Key.Count(c => c == ','))
                .ThenByDescending(s => s.Value[0] / (s.Value[0] + (double)s.Value[1]))
        )
        {
            var (roles, winloss) = stats;
            var winPercentage = 100 * winloss[0] / (double)(winloss[0] + winloss[1]);
            Console.WriteLine(
                $"{roles}: {winloss[0]} wins, {winloss[1]} losses ({winPercentage:0.0}%)"
            );
        }
    }

    private static (PandemicGame, IEnumerable<IEvent>) PlayGame(PandemicGame game, ILiveAgent agent)
    {
        var numActions = 0;
        var events = new List<IEvent>();

        for (; numActions < 1000 && !game.IsOver; numActions++)
        {
            if (numActions == 999)
            {
                throw new InvalidOperationException("didn't expect this many turns");
            }

            var command = agent.NextCommand(game);
            game = game.Do(command, events);
        }

        return (game, events);
    }
}
