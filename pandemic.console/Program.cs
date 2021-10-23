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
        // todo: clean up this file
        static void Main(string[] args)
        {
            // PlaySingleRandomGameVerbose();
            PlayRandomGamesUntilWon();
            // var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            // {
            //     Difficulty = Difficulty.Introductory,
            //     Roles = new[] {Role.Medic, Role.Scientist}
            // });
            // FindWinWithSolver(game, new DfsAgent()); // ~1M games/8 seconds
            // FindWinWithSolver(game, new DfsWithHeuristicsAgent());  // ~1M games/8 seconds
        }

        private static void FindWinWithSolver(PandemicGame game, IPandemicGameSolver solver)
        {
            var commands = solver.CommandsToWin(new PandemicSpielGameState(game)).ToList();

            if (commands.Any())
            {
                Console.WriteLine("found win!");
                foreach (var command in commands)
                {
                    Console.WriteLine(command);
                }
            }
            else
            {
                Console.WriteLine("no win found");
            }
        }

        private static void PlayRandomGamesUntilWon()
        {
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            };

            var won = false;
            var sw = Stopwatch.StartNew();
            var lastNumGames = 0;
            var lastTime = sw.Elapsed;
            var stats = new GameStats();

            var state = new PandemicSpielGameState(PandemicGame.CreateUninitialisedGame());
            var events = Enumerable.Empty<IEvent>();

            for (var i = 0; !won && i < 1000; i++)
            {
                (state, events) = PlayRandomGame(options, stats);
                won = state.Game.IsWon;

                if ((sw.Elapsed - lastTime).TotalMilliseconds > 1000)
                {
                    Console.WriteLine($"{i - lastNumGames} games/sec");
                    lastTime = sw.Elapsed;
                    lastNumGames = i;
                }
            }

            // PrintEventsAndState(events, state);
            PrintStats(stats);
        }

        private static void PlaySingleRandomGameVerbose()
        {
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] {Role.Medic, Role.Scientist}
            };

            var (state, events) = PlayRandomGame(options, new GameStats());

            PrintEventsAndState(events, state);
        }

        private static void PrintEventsAndState(IEnumerable<IEvent> events, PandemicSpielGameState state)
        {
            Console.WriteLine("Game over! Events:");
            Console.WriteLine();
            foreach (var @event in events)
            {
                Console.WriteLine(@event);
            }

            Console.WriteLine();
            Console.WriteLine("Final state:");
            Console.WriteLine();
            Console.WriteLine(state);
        }

        private static (PandemicSpielGameState, IEnumerable<IEvent>) PlayRandomGame(
            NewGameOptions options,
            GameStats stats)
        {
            var random = new Random();
            var (game, events) = PandemicGame.CreateNewGame(options);
            var state = new PandemicSpielGameState(game);
            var numActions = 0;

            for (; numActions < 1000 && !state.IsTerminal; numActions++)
            {
                if (numActions == 999) throw new InvalidOperationException("didn't expect this many turns");

                var legalActions = state.LegalActions().ToList();
                if (!legalActions.Any())
                {
                    throw new InvalidOperationException("oh no!");
                }

                stats.AddLegalActionCount(legalActions.Count);

                var action = RandomChoice(state.LegalActions(), random);
                events.AddRange(state.ApplyAction(action));
            }

            stats.AddNumActionsInGame(numActions);

            return (state, events);
        }

        private static T RandomChoice<T>(IEnumerable<T> items, Random random)
        {
            var itemList = items.ToList();
            if (!itemList.Any()) throw new InvalidOperationException("no items to choose from!");

            var idx = random.Next(0, itemList.Count);
            return itemList[idx];
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

        private class GameStats
        {
            /// <summary>
            /// {num actions : count}
            /// </summary>
            public readonly Dictionary<int, int> ActionsPerGameCounts = new();

            /// <summary>
            /// {num legal actions : count}
            /// </summary>
            public readonly Dictionary<int, int> LegalActionCounts = new();

            public void AddNumActionsInGame(int numActions)
            {
                if (ActionsPerGameCounts.ContainsKey(numActions))
                    ActionsPerGameCounts[numActions]++;
                else
                    ActionsPerGameCounts[numActions] = 1;
            }

            public void AddLegalActionCount(int numLegalActions)
            {
                if (LegalActionCounts.ContainsKey(numLegalActions))
                    LegalActionCounts[numLegalActions]++;
                else
                    LegalActionCounts[numLegalActions] = 1;
            }
        }
    }
}
