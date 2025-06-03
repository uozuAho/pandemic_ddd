namespace pandemic.console;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using agents;
using agents.GreedyBfs;
using Aggregates.Game;
using Commands;
using test.Utils;
using utils;
using Values;
using SearchNode = agents.GreedyBfs.SearchNode;

/// <summary>
/// Plays games with different agents in a way that allows their performance to be compared
/// </summary>
internal static class AgentComparer
{
    public static void Run()
    {
        // RunRandomGames();
        // RunGreedyGames(TimeSpan.FromSeconds(5));
        RunGreedyBestFirst(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), 1);
        // RunDfs();
        // RunDfsWithHeuristics();
    }

    private static void RunRandomGames()
    {
        var totalRunTime = TimeSpan.FromSeconds(5);
        var random = new Random();

        var totalTimer = Stopwatch.StartNew();
        var numGames = 0;
        var numWins = 0;
        var statesVisited = 0;

        Console.WriteLine("Running random games...");

        while (totalTimer.Elapsed < totalRunTime)
        {
            numGames++;
            var game = NewGame();

            while (!game.IsOver)
            {
                statesVisited++;
                var action = random.Choice(game.LegalCommands());
                (game, _) = game.Do(action);
            }

            if (game.IsWon)
            {
                numWins++;
            }
        }

        Console.WriteLine(
            $"{numGames} games played. {numWins} wins. {statesVisited} states explored."
        );
    }

    /// <summary>
    /// Use the greedy agent to play games by picking the 'best' command
    /// on a turn by turn basis (no search).
    /// </summary>
    private static void RunGreedyGames(TimeSpan timeLimit)
    {
        Console.WriteLine("Running greedy games...");

        var timer = Stopwatch.StartNew();
        var totalGames = 0;
        var wins = 0;
        var losses = new Dictionary<string, int>();

        while (timer.Elapsed < timeLimit)
        {
            var game = NewGame();
            var agent = new GreedyAgent();
            while (!game.IsOver)
            {
                (game, _) = game.Do(agent.NextCommand(game));
            }

            if (game.IsWon)
            {
                wins++;
            }
            else
            {
                var lossReason = game.LossReason;
                if (!losses.TryGetValue(lossReason, out var value))
                {
                    value = 0;
                    losses[lossReason] = value;
                }

                losses[lossReason] = ++value;
            }

            totalGames++;
        }

        Console.WriteLine($"Total games: {totalGames}");
        Console.WriteLine($"Wins: {wins}");
        Console.WriteLine("Losses:");
        foreach (var (reason, count) in losses)
        {
            Console.WriteLine($"{reason} ({count})");
        }
    }

    private static void RunGreedyBestFirst(
        TimeSpan totalRunTime,
        TimeSpan maxGameTime,
        int numThreads
    )
    {
        RunGreedyBestFirstSingleThread(totalRunTime, maxGameTime);

        // var tasks = Enumerable.Range(0, numThreads)
        //     .Select(_ => Task.Run(() => RunGreedyBestFirstSingleThread(totalRunTime, maxGameTime)));
        //
        // Task.WaitAll(tasks.ToArray());
    }

    private static void RunGreedyBestFirstSingleThread(TimeSpan totalRunTime, TimeSpan maxGameTime)
    {
        var totalTimer = Stopwatch.StartNew();
        var numGames = 0;
        var numWins = 0;
        var statesVisited = 0;
        Console.WriteLine("Running greedy best first...");

        SearchNode? bestNode = null;
        SearchNode? worstNode = null;
        while (totalTimer.Elapsed < totalRunTime)
        {
            numGames++;
            var game = NewGame();
            var searcher = new GreedyBestFirstSearch(game);

            var gameTimer = Stopwatch.StartNew();

            while (!searcher.IsFinished)
            {
                var node = searcher.Step();
                statesVisited++;

                Debug.Assert(node != null, nameof(node) + " != null");

                if (worstNode == null || node.Score < worstNode.Score)
                {
                    worstNode = node;
                }

                if (bestNode == null || node.Score > bestNode.Score)
                {
                    bestNode = node;
                }

                if (gameTimer.Elapsed > maxGameTime)
                {
                    break;
                }
            }

            if (searcher.CurrentState.IsWon)
            {
                numWins++;
            }
        }

        Console.WriteLine(
            $"{numGames} games played. {numWins} wins. {statesVisited} states explored."
        );
    }

    private static void RunDfs()
    {
        Console.WriteLine("Running DFS games...");

        var game = NewGame();
        var dfs = new DfsAgent();

        try
        {
            _ = dfs.CommandsToWin(game, TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException) { }
    }

    private static void RunDfsWithHeuristics()
    {
        Console.WriteLine("Running DFS with heuristics games...");

        var game = NewGame();
        var dfs = new DfsWithHeuristicsAgent();

        try
        {
            _ = dfs.CommandsToWin(game, TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException) { }
    }

    private static PandemicGame NewGame()
    {
        var options = NewGameOptionsGenerator.RandomOptions() with
        {
            Roles = new[] { Role.Medic, Role.Dispatcher },
            Difficulty = Difficulty.Introductory,
            CommandGenerator = new SensibleCommandGenerator(),
        };

        var (game, _) = PandemicGame.CreateNewGame(options);

        return game with
        {
            SelfConsistencyCheckingEnabled = false,
        };
    }
}
