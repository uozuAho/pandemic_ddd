using System;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.test
{
    public class GameSetup
    {
        [TestCase(Difficulty.Introductory)]
        [TestCase(Difficulty.Normal)]
        [TestCase(Difficulty.Heroic)]
        public void Do_all_the_stuff_to_start_a_game(Difficulty difficulty)
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = difficulty,
                Roles = new[] { Role.Medic, Role.Scientist }
            });


            Assert.AreEqual(difficulty, game.Difficulty);
            Assert.AreEqual(2, game.InfectionRate);
            Assert.AreEqual(0, game.OutbreakCounter);
            Assert.AreEqual(2, game.Players.Count);
            Assert.AreEqual(48, game.InfectionDrawPile.Count);
            Assert.AreEqual(0, game.InfectionDiscardPile.Count);
            Assert.AreEqual(48 + PandemicGame.NumberOfEpidemicCards(difficulty) - 8, game.PlayerDrawPile.Count);
            Assert.IsTrue(game.Players.All(p => p.Hand.Count == 4));
            Assert.IsFalse(game.IsOver);
        }

        // todo: different player numbers draw different numbers of cards
    }
}
