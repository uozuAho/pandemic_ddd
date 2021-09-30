using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;

namespace pandemic.test
{
    class SpielRandomAgentTests
    {
        [Test]
        public void asdf()
        {
            var random = new Random();
            var state = new PandemicSpielGameState(PandemicGame.CreateUninitialisedGame());

            for (var i = 0; i < 1000 && !state.IsTerminal; i++)
            {
                var action = Choice(state.LegalActions(), random);
                state.ApplyAction(action);
            }

            Assert.IsTrue(state.IsTerminal, "Expected to reach terminal state in under 1000 actions");
        }

        private static T Choice<T>(IEnumerable<T> items, Random random)
        {
            var itemList = items.ToList();
            var idx = random.Next(0, itemList.Count);
            return itemList[idx];
        }
    }
}
