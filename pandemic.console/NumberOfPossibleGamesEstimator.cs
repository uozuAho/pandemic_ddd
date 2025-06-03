namespace pandemic.console;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aggregates.Game;
using Commands;
using utils;
using Values;

internal class NumberOfPossibleGamesEstimator
{
    public static void Estimate()
    {
        var stats = new GameStats();
        Console.WriteLine("Playing games...");
        PlayRandomGamesFor(TimeSpan.FromSeconds(3), stats);
        PrintStats(stats);
    }

    private static void PlayRandomGamesFor(TimeSpan time, GameStats stats)
    {
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
        {
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist },
            };
            var (game, _) = PandemicGame.CreateNewGame(options);

            game = PlayRandomGame(game, stats);

            if (game.IsWon)
            {
                stats.Wins++;
            }

            if (game.IsLost)
            {
                stats.RecordLoss(game.LossReason);
            }

            stats.GamesPlayed++;
        }

        stats.RunTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
    }

    private static PandemicGame PlayRandomGame(PandemicGame game, GameStats stats)
    {
        var numActions = 0;
        var random = new Random();

        for (; numActions < 1000 && !game.IsOver; numActions++)
        {
            if (numActions == 999)
            {
                throw new InvalidOperationException("didn't expect this many turns");
            }

            var legalActions = PlayerCommandGenerator.AllLegalCommands(game).ToList();
            stats.AddLegalActionCount(legalActions.Count);

            var action = random.Choice(legalActions);
            var (updatedGame, _) = game.Do(action);
            game = updatedGame;
        }

        stats.AddNumActionsInGame(numActions);
        return game;
    }

    private static void PrintStats(GameStats stats)
    {
        Console.WriteLine(
            $"ran {stats.GamesPlayed} games in {stats.RunTime} "
                + $"({stats.GamesPlayed / stats.RunTime.TotalSeconds} games/sec)"
        );

        Console.WriteLine($"{stats.Wins} wins");
        Console.WriteLine($"{stats.Losses} losses");
        Console.WriteLine("loss reasons:");
        foreach (var (reason, count) in stats.LossReasons)
        {
            Console.WriteLine($"  {reason}: {count}");
        }

        var avgActionsPerGame =
            stats.ActionsPerGameCounts.Select(a => a.Key * a.Value).Sum() / stats.GamesPlayed;
        Console.WriteLine("number of actions per game:");
        Console.WriteLine($"min: {stats.ActionsPerGameCounts.MinBy(a => a.Key).Key}");
        Console.WriteLine($"avg: {avgActionsPerGame}");
        Console.WriteLine($"max: {stats.ActionsPerGameCounts.MaxBy(a => a.Key).Key}");

        var totalTurns = stats.LegalActionCounts.Sum(a => a.Value);
        var avgLegalActionsPerTurn = stats.LegalActionCounts.Sum(a => a.Key * a.Value) / totalTurns;
        Console.WriteLine("number of legal actions per turn:");
        Console.WriteLine($"min: {stats.LegalActionCounts.MinBy(a => a.Key).Key}");
        Console.WriteLine($"avg: {avgLegalActionsPerTurn}");
        Console.WriteLine($"max: {stats.LegalActionCounts.MaxBy(a => a.Key).Key}");

        Console.WriteLine(
            "Num possible games (avg. branch factor ^ avg. depth) = "
                + $"({avgLegalActionsPerTurn} ^ {avgActionsPerGame}) = "
                + $"{Math.Pow(avgLegalActionsPerTurn, avgActionsPerGame)}"
        );
    }

    private class GameStats
    {
        /// <summary>
        /// {num actions : count}
        /// </summary>
        public readonly Dictionary<int, int> ActionsPerGameCounts = [];

        /// <summary>
        /// {num legal actions : count}
        /// </summary>
        public readonly Dictionary<int, int> LegalActionCounts = [];

        public int GamesPlayed { get; set; }
        public TimeSpan RunTime { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public Dictionary<string, int> LossReasons { get; set; } = [];

        public void AddNumActionsInGame(int numActions)
        {
            ActionsPerGameCounts[numActions] = ActionsPerGameCounts.TryGetValue(numActions, out var value) ? ++value : 1;
        }

        public void AddLegalActionCount(int numLegalActions)
        {
            LegalActionCounts[numLegalActions] = LegalActionCounts.TryGetValue(numLegalActions, out var value) ? ++value : 1;
        }

        public void RecordLoss(string lossReason)
        {
            Losses++;
            LossReasons[lossReason] = LossReasons.TryGetValue(lossReason, out var value) ? ++value : 1;
        }
    }
}
