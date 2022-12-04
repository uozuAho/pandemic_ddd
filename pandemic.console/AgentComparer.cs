using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using pandemic.agents;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Values;
using utils;

namespace pandemic.console
{
    /// <summary>
    /// Plays games with different agents in a way that allows their performance to be compared
    /// </summary>
    internal static class AgentComparer
    {
        public static void Run()
        {
            RunRandomGames();
            RunGreedyBestFirst();
            RunDfs();
            RunDfsWithHeuristics();
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

        private static void RunGreedyBestFirst()
        {
            var totalRunTime = TimeSpan.FromSeconds(5);
            // seems to find a win in under 1s, or never
            var maxGameTime = TimeSpan.FromSeconds(1);

            // increasing threads reduces single thread performance :(
            var numThreads = Environment.ProcessorCount / 3;
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

            while (totalTimer.Elapsed < totalRunTime)
            {
                numGames++;
                var game = NewGame();
                var searchProblem = new PandemicSearchProblem(game, new PlayerCommandGenerator());
                var searcher = new GreedyBestFirstSearch(searchProblem);

                var gameTimer = Stopwatch.StartNew();

                while (!searcher.IsFinished)
                {
                    searcher.Step();
                    statesVisited++;

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

            Console.WriteLine($"{numGames} games played. {numWins} wins. {statesVisited} states explored.");
            Console.WriteLine("Win times:");
            foreach (var time in winTimes)
            {
                Console.WriteLine(time);
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
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist }
            });

            return game;
        }
    }
}
