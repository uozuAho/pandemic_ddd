using NUnit.Framework;
using pandemic.Aggregates;

namespace pandemic.test
{
    class SpielRandomAgentTests
    {
        [Test]
        public void asdf()
        {
            var state = new PandemicSpielGameState(PandemicGame.CreateUninitialisedGame());

            for (var i = 0; i < 1000 && !state.IsTerminal; i++)
            {
                state.ApplyAction(0);
            }

            Assert.IsTrue(state.IsTerminal, "Expected to reach terminal state in under 1000 actions");
        }
    }
}
