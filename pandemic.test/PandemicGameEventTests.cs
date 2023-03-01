using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Events;
using pandemic.test.Utils;
using pandemic.Values;

namespace pandemic.test
{
    class PandemicGameEventTests
    {
        [Test]
        [Repeat(10)]
        public void State_built_from_events_is_same_as_final_state()
        {
            var options = NewGameOptionsGenerator.RandomOptions();

            var commandGenerator = new PlayerCommandGenerator();
            var random = new Random();
            PandemicGame game;
            List<IEvent> events;
            (game, events) = PandemicGame.CreateNewGame(options);

            for (var i = 0; i < 1000 && !game.IsOver; i++)
            {
                var legalActions = commandGenerator.LegalCommands(game).ToList();
                if (!legalActions.Any())
                {
                    Assert.Fail($"No legal actions! State: \n\n{game}\n\n Events:\n{string.Join('\n', events)}");
                }
                var action = RandomChoice(legalActions, random);
                var (updatedGame, newEvents) = game.Do(action);
                game = updatedGame;
                events.AddRange(newEvents);
            }

            var builtFromEvents = PandemicGame.FromEvents(events);

            Assert.IsTrue(game.IsSameStateAs(builtFromEvents));
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
