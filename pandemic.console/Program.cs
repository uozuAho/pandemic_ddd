using System;
using System.Collections.Generic;
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
            var random = new Random();
            var options = new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            };
            PandemicGame game;
            List<IEvent> events;
            (game, events) = PandemicGame.CreateNewGame(options);
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

        private static T RandomChoice<T>(IEnumerable<T> items, Random random)
        {
            var itemList = items.ToList();
            if (!itemList.Any()) throw new InvalidOperationException("no items to choose from!");

            var idx = random.Next(0, itemList.Count);
            return itemList[idx];
        }
    }
}
