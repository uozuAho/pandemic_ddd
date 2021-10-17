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

            var state = new PandemicSpielGameState(PandemicGame.CreateUninitialisedGame());
            var events = Enumerable.Empty<IEvent>();

            for (var i = 0; !won; i++)
            {
                (state, events) = PlayRandomGame(options);
                won = state.Game.IsWon;

                if ((sw.Elapsed - lastTime).TotalMilliseconds > 1000)
                {
                    Console.WriteLine($"{i - lastNumGames} games/sec");
                    lastTime = sw.Elapsed;
                    lastNumGames = i;
                }
            }

            PrintEventsAndState(events, state);
        }

        private static void PlaySingleRandomGameVerbose()
        {
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] {Role.Medic, Role.Scientist}
            };

            var (state, events) = PlayRandomGame(options);

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

        private static (PandemicSpielGameState, IEnumerable<IEvent>) PlayRandomGame(NewGameOptions options)
        {
            var random = new Random();
            var (game, events) = PandemicGame.CreateNewGame(options);
            var state = new PandemicSpielGameState(game);

            for (var i = 0; i < 1000 && !state.IsTerminal; i++)
            {
                var legalActions = state.LegalActions();
                if (!legalActions.Any())
                {
                    throw new InvalidOperationException("oh no!");
                }

                var action = RandomChoice(state.LegalActions(), random);
                events.AddRange(state.ApplyAction(action));
            }

            return (state, events);
        }

        private static T RandomChoice<T>(IEnumerable<T> items, Random random)
        {
            var itemList = items.ToList();
            if (!itemList.Any()) throw new InvalidOperationException("no items to choose from!");

            var idx = random.Next(0, itemList.Count);
            return itemList[idx];
        }
    }
}
