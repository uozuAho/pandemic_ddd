using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using pandemic.agents;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.test.Utils;
using pandemic.Values;
using utils;
using SearchNode = pandemic.agents.GreedyBfs.SearchNode;

namespace pandemic.console
{
    /// <summary>
    /// Plays games with different agents in a way that allows their performance to be compared
    /// </summary>
    internal static class AgentComparer
    {
        public static void Run()
        {
            // RunRandomGames();
            RunGreedyGames(TimeSpan.FromSeconds(5));
            // RunGreedyBestFirst();
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
                var searchProblem = new PandemicSearchProblem(game, new PlayerCommandGenerator());

                while (!game.IsOver)
                {
                    statesVisited++;
                    var action = random.Choice(searchProblem.GetActions(game));
                    game = searchProblem.DoAction(game, action);
                }

                if (game.IsWon)
                {
                    numWins++;
                }
            }

            Console.WriteLine($"{numGames} games played. {numWins} wins. {statesVisited} states explored.");
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

                if (game.IsWon) wins++;
                else
                {
                    var lossReason = game.LossReason;
                    if (!losses.ContainsKey(lossReason)) losses[lossReason] = 0;
                    losses[lossReason]++;
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

        private static void RunGreedyBestFirst()
        {
            var totalRunTime = TimeSpan.FromSeconds(5);
            var maxGameTime = TimeSpan.FromSeconds(1);

            // increasing threads reduces single thread performance :(
            var numThreads = Environment.ProcessorCount / 2;
            var tasks = Enumerable.Range(0, numThreads)
                .Select(_ => Task.Run(() => RunGreedyBestFirstSingleThread(totalRunTime, maxGameTime)));

            Task.WaitAll(tasks.ToArray());
        }

        private static void RunGreedyBestFirstSingleThread(
            TimeSpan totalRunTime,
            TimeSpan maxGameTime)
        {
            var totalTimer = Stopwatch.StartNew();
            var numGames = 0;
            var numWins = 0;
            var statesVisited = 0;
            var winTimes = new List<TimeSpan>();

            Console.WriteLine("Running greedy best first...");

            SearchNode? bestNode = null;
            SearchNode? worstNode = null;
            while (totalTimer.Elapsed < totalRunTime)
            {
                numGames++;
                var game = NewGame();
                var searchProblem = new PandemicSearchProblem(game, new PlayerCommandGenerator());
                var searcher = new GreedyBestFirstSearch(searchProblem);

                var gameTimer = Stopwatch.StartNew();

                while (!searcher.IsFinished)
                {
                    var node = searcher.Step();
                    statesVisited++;

                    if (worstNode == null || node.Score < worstNode.Score) worstNode = node;
                    if (bestNode == null || node.Score > bestNode.Score) bestNode = node;

                    if (gameTimer.Elapsed > maxGameTime)
                    {
                        break;
                    }
                }

                if (searcher.CurrentState.IsWon)
                {
                    numWins++;
                    winTimes.Add(gameTimer.Elapsed);
                }
            }

            Console.WriteLine("========================================");
            Console.WriteLine($"{numGames} games played. {numWins} wins. {statesVisited} states explored.");
            // Console.WriteLine("Best");
            // Console.WriteLine(bestNode.State);
            // Console.WriteLine();
            // Console.WriteLine("------------------");
            // Console.WriteLine("Worst");
            // Console.WriteLine(worstNode.State);
            Console.WriteLine("  Win times:");
            foreach (var time in winTimes)
            {
                Console.WriteLine($"  {time}");
            }
        }

        private static void RunDfs()
        {
            Console.WriteLine("Running DFS games...");

            var game = NewGame();
            var dfs = new DfsAgent();

            try
            {
                dfs.CommandsToWin(game, TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
            }
        }

        private static void RunDfsWithHeuristics()
        {
            Console.WriteLine("Running DFS with heuristics games...");

            var game = NewGame();
            var dfs = new DfsWithHeuristicsAgent();

            try
            {
                dfs.CommandsToWin(game, TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
            }
        }

        private static PandemicGame NewGame()
        {
            var options = NewGameOptionsGenerator.RandomOptions() with
            {
                Difficulty = Difficulty.Introductory,
                CommandGenerator = new SensibleCommandGenerator()
            };

            var (game, _) = PandemicGame.CreateNewGame(options);

            return game with { SelfConsistencyCheckingEnabled = false };
        }
    }
}
