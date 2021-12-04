using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.console
{
    class Program
    {
        static void Main(string[] args)
        {
            // single playthrough
            var gameOptions = new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist }
            };
            var (game, events) = PandemicGame.CreateNewGame(gameOptions);
            var stats = new GameStats();

            // var (endState, events2) = SingleGame.PlayRandomGame(game, stats);
            // events.AddRange(events2);
            //
            // Console.WriteLine("events:");
            // PrintEvents(events);
            //
            // Console.WriteLine();
            // Console.WriteLine("state:");
            // PrintState(endState);
            // PrintStats(stats);

            // WinFinder.FindWinWithSolver(game, new DfsAgent());
            // PlayRandomGamesUntilWon();
            // FindWinWithSolver(game, new DfsWithHeuristicsAgent());  // ~1M games/8 seconds
            // PlayInfiniteMctsGames();
            // RandomPlaythroughDrawer.DoIt();
            // DfsDrawer.DrawSearch(game);
        }

        // private static void PlayRandomGamesUntilWon()
        // {
        //     var options = new NewGameOptions
        //     {
        //         Difficulty = Difficulty.Introductory,
        //         Roles = new[] { Role.Medic, Role.Scientist }
        //     };
        //
        //     var won = false;
        //     var sw = Stopwatch.StartNew();
        //     var lastNumGames = 0;
        //     var lastTime = sw.Elapsed;
        //     var stats = new Program.GameStats();
        //
        //     var state = new PandemicSpielGameState(PandemicGame.CreateUninitialisedGame());
        //     var events = Enumerable.Empty<IEvent>();
        //
        //     for (var i = 0; !won && i < int.MaxValue; i++)
        //     {
        //         (state, events) = PlayRandomGame(options, stats);
        //         won = state.Game.IsWon;
        //
        //         if ((sw.Elapsed - lastTime).TotalMilliseconds > 1000)
        //         {
        //             Console.WriteLine($"{i - lastNumGames} games/sec");
        //             lastTime = sw.Elapsed;
        //             lastNumGames = i;
        //         }
        //     }
        //
        //     // PrintEventsAndState(events, state);
        //     PrintStats(stats);
        // }

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

        private static void PrintState(PandemicSpielGameState state)
        {
            Console.WriteLine(state);
        }

        private static void PrintEvents(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                Console.WriteLine(@event);
            }
        }

        private static void PrintStats(GameStats stats)
        {
            Console.WriteLine("actions, count");
            foreach (var (actions, count) in stats.ActionsPerGameCounts.OrderBy(c => c.Key))
            {
                Console.WriteLine($"{actions}, {count}");
            }

            Console.WriteLine("legal actions, count");
            foreach (var (actions, count) in stats.LegalActionCounts.OrderBy(c => c.Key))
            {
                Console.WriteLine($"{actions}, {count}");
            }
        }
    }
}
