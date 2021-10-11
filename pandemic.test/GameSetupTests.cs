using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.test.Utils;
using pandemic.Values;

namespace pandemic.test
{
    public class GameSetup
    {
        [TestCaseSource(typeof(NewGameOptionsGenerator), nameof(NewGameOptionsGenerator.AllOptions))]
        public void Do_all_the_stuff_to_start_a_game(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            Assert.AreEqual(options.Difficulty, game.Difficulty);
            Assert.AreEqual(2, game.InfectionRate);
            Assert.AreEqual(0, game.OutbreakCounter);
            Assert.AreEqual(2, game.Players.Count);
            Assert.AreEqual(48, game.InfectionDrawPile.Count);
            Assert.AreEqual(0, game.InfectionDiscardPile.Count);
            Assert.AreEqual(48 + PandemicGame.NumberOfEpidemicCards(options.Difficulty) - 8, game.PlayerDrawPile.Count);
            Assert.IsTrue(game.Players.All(p => p.Hand.Count == 4));
            Assert.IsFalse(game.IsOver);
        }
    }
}
