using System;
using System.Collections.Generic;
using System.Diagnostics;
using pandemic.agents;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.console
{
    class Program
    {
        static void Main(string[] args)
        {
            // SingleGame.PlayGameAndPrintPlaythrough(new GreedyAgent());
            // AgentComparer.Run();
            WinLossStats.PlayGamesAndPrintWinLossStats(new GreedyAgent(), TimeSpan.FromSeconds(5));
            // PlayInteractiveGame();

            // NumberOfPossibleGamesEstimator.Estimate();
            // WinFinder.FindWinWithSolver(CreateNewGame(), new DfsAgent());
            // WinFinder.FindWinWithSolver(CreateNewGame(), new DfsWithHeuristicsAgent());
            // PlayInfiniteMctsGames();
            // RandomPlaythroughDrawer.DoIt();
            // DfsDrawer.DrawSearch(CreateNewGame());
            // HeuristicDfsDrawer.DrawSearch(CreateNewGame());
            // BfsRunner.Run();
            // BfsRunner.Draw(500);
        }

        private static void PlayInteractiveGame()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Dispatcher },
                CommandGenerator = new SensibleCommandGenerator()
            });
            InteractiveGame.Play(game);
        }

        private static void PlayInfiniteMctsGames()
        {
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            };

            const int numSimulations = 3;
            const int numRollouts = 3;
            var agent = new MctsAgent(numSimulations, numRollouts);

            var sw = Stopwatch.StartNew();
            var numGames = 0;
            var wins = 0;
            var losses = 0;

            while (true)
            {
                var (game, _) = PandemicGame.CreateNewGame(options);
                var state = new PandemicSpielGameState(game);
                var events = new List<IEvent>();

                for (var step = 0; step < 1000 && !state.IsTerminal; step++)
                {
                    if (step == 999) throw new InvalidOperationException("didn't expect this many turns");
                    var action = agent.Step(state);
                    events.AddRange(state.ApplyAction(action));
                }

                numGames++;
                if (state.IsWin) wins++;
                else losses++;

                if (sw.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"{numGames} games/sec. {wins} wins, {losses} losses");
                    numGames = 0;
                    sw.Restart();
                }
            }
        }
    }
}
