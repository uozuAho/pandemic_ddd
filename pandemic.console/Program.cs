using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            for (var i = 0; !won; i++)
            {
                var (state, _) = PlayRandomGame(options);
                won = state.Game.IsWon;

                if ((sw.Elapsed - lastTime).TotalMilliseconds > 1000)
                {
                    Console.WriteLine($"{i - lastNumGames} games/sec");
                    lastTime = sw.Elapsed;
                    lastNumGames = i;
                }
            }

            Console.WriteLine("won!");
        }

        private static void PlaySingleRandomGameVerbose()
        {
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] {Role.Medic, Role.Scientist}
            };

            var (state, events) = PlayRandomGame(options);

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
