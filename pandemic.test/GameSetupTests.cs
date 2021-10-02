using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.test
{
    public class GameSetup
    {
        [Test]
        public void Do_all_the_stuff_to_start_a_game()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.AreEqual(Difficulty.Normal, game.Difficulty);
            Assert.AreEqual(2, game.InfectionRate);
            Assert.AreEqual(0, game.OutbreakCounter);
            Assert.AreEqual(1, game.Players.Count);
            Assert.AreEqual(48, game.InfectionDrawPile.Count);
            Assert.AreEqual(0, game.InfectionDiscardPile.Count);
            Assert.IsFalse(game.IsOver);
        }
    }
}
