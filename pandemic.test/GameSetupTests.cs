using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.test.Utils;
using pandemic.Values;

namespace pandemic.test
{
    public class GameSetup
    {
        [Test]
        public void Do_all_the_stuff_to_start_a_game()
        {
            var state = PandemicGame.FromEvents(GameBuilder.InitialiseNewGame());

            Assert.AreEqual(Difficulty.Normal, state.Difficulty);
            Assert.AreEqual(2, state.InfectionRate);
            Assert.AreEqual(0, state.OutbreakCounter);
            Assert.AreEqual(1, state.Players.Count);
            Assert.AreEqual(48, state.InfectionDrawPile.Count);
            Assert.AreEqual(0, state.InfectionDiscardPile.Count);
        }
    }
}
