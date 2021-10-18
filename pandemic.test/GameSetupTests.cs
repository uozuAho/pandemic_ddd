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

            var numberOfPlayers = options.Roles.Count;
            var numberOfCardsPerPlayer = PandemicGame.InitialPlayerHandSize(numberOfPlayers);
            var numberOfEpidemicCards = PandemicGame.NumberOfEpidemicCards(options.Difficulty);

            Assert.AreEqual(options.Difficulty, game.Difficulty);
            Assert.AreEqual(2, game.InfectionRate);
            Assert.AreEqual(0, game.OutbreakCounter);
            Assert.AreEqual(options.Roles.Count, game.Players.Count);
            Assert.AreEqual(48, game.InfectionDrawPile.Count);
            Assert.AreEqual(0, game.InfectionDiscardPile.Count);
            Assert.AreEqual(6, game.ResearchStationPile);

            // todo: enable these once epidemic cards are inserted
            // Assert.AreEqual(48 + numberOfEpidemicCards - numberOfPlayers * numberOfCardsPerPlayer, game.PlayerDrawPile.Count);
            // Assert.AreEqual(numberOfEpidemicCards, game.PlayerDrawPile.Count(c => c is EpidemicCard));

            // todo: no epidemic cards in player hands
            Assert.IsTrue(game.Players.All(p => p.Hand.Count == numberOfCardsPerPlayer));
            Assert.IsTrue(game.CityByName("Atlanta").HasResearchStation);
            Assert.IsTrue(game.CureDiscovered.All(kv => !kv.Value));
            Assert.IsFalse(game.IsOver);
        }
    }
}
