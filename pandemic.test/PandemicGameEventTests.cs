using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.test.Utils;
using pandemic.Values;

namespace pandemic.test
{
    class PandemicGameEventTests
    {
        [TestCaseSource(typeof(NewGameOptionsGenerator), nameof(NewGameOptionsGenerator.AllOptions))]
        public void State_built_from_events_is_same_as_final_state(NewGameOptions options)
        {
            var random = new Random();
            PandemicGame game;
            List<IEvent> events;
            (game, events) = PandemicGame.CreateNewGame(options);
            var state = new PandemicSpielGameState(game);
            var tempgame2 = PandemicGame.FromEvents(events);
            Assert.IsTrue(state.Game.IsSameStateAs(tempgame2));

            for (var i = 0; i < 1000 && !state.IsTerminal; i++)
            {
                var legalActions = state.LegalActions();
                if (!legalActions.Any())
                {
                    Assert.Fail($"No legal actions! State: \n\n{state}\n\n Events:\n{string.Join('\n', events)}");
                }
                var action = RandomChoice(state.LegalActions(), random);
                events.AddRange(state.ApplyAction(action));
                var tempgame = PandemicGame.FromEvents(events);
                var asdf = state.Game.IsSameStateAs(tempgame);
                Assert.IsTrue(state.Game.IsSameStateAs(tempgame));
            }

            var builtFromEvents = PandemicGame.FromEvents(events);

            Assert.IsTrue(state.Game.IsSameStateAs(builtFromEvents));
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
