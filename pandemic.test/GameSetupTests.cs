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
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            const int numberOfEpidemicCards = 5;

            Assert.AreEqual(Difficulty.Normal, game.Difficulty);
            Assert.AreEqual(2, game.InfectionRate);
            Assert.AreEqual(0, game.OutbreakCounter);
            Assert.AreEqual(2, game.Players.Count);
            Assert.AreEqual(48, game.InfectionDrawPile.Count);
            Assert.AreEqual(0, game.InfectionDiscardPile.Count);
            Assert.AreEqual(48 + numberOfEpidemicCards - 8, game.PlayerDrawPile.Count);
            // todo: players have 4 cards in hand
            Assert.IsFalse(game.IsOver);
        }

        // todo: different player numbers draw different numbers of cards
        // todo: different difficulties have different number of epidemic cards
    }
}
