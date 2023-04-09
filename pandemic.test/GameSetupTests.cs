using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.test.Utils;
using pandemic.Values;
using Shouldly;
using utils;

namespace pandemic.test
{
    public class GameSetup
    {
        [Test]
        [Repeat(10)]
        public void Initial_game_state_is_correct()
        {
            var options = NewGameOptionsGenerator.RandomOptions();

            var (game, _) = PandemicGame.CreateNewGame(options);
            var numberOfPlayers = options.Roles.Count;
            var numberOfCardsPerPlayer = PandemicGame.InitialPlayerHandSize(numberOfPlayers);
            var numberOfEpidemicCards = PandemicGame.NumberOfEpidemicCards(options.Difficulty);

            // board
            Assert.AreEqual(options.Difficulty, game.Difficulty);
            Assert.AreEqual(2, game.InfectionRate);
            Assert.AreEqual(0, game.OutbreakCounter);
            Assert.AreEqual(5, game.ResearchStationPile);
            game.CuresDiscovered.ShouldBeEmpty();
            Assert.IsFalse(game.IsOver);

            // decks
            Assert.That(game.PlayerDrawPile.Count, Is.EqualTo(
                StandardGameBoard.NumberOfCities
                + StandardGameBoard.NumberOfSpecialEventCards
                + numberOfEpidemicCards
                - numberOfPlayers * numberOfCardsPerPlayer));
            Assert.That(game.PlayerDrawPile.Cards.Count(c => c is EpidemicCard) == numberOfEpidemicCards);
            AssertEpidemicCardsAreDistributed(game.PlayerDrawPile, game.Difficulty);

            // cities
            Assert.That(game.CityByName("Atlanta").HasResearchStation);

            // initial infection
            Assert.That(game.InfectionDrawPile.Count, Is.EqualTo(StandardGameBoard.NumberOfCities - 9));
            Assert.AreEqual(9, game.InfectionDiscardPile.Count);
            Assert.That(game.Cubes.Total(), Is.EqualTo(
                96
                - 3 * 3
                - 3 * 2
                - 3 * 1));
            Assert.That(game.Cities.Count(c => c.MaxNumCubes() == 3), Is.EqualTo(3));
            Assert.That(game.Cities.Count(c => c.MaxNumCubes() == 2), Is.EqualTo(3));
            Assert.That(game.Cities.Count(c => c.MaxNumCubes() == 1), Is.EqualTo(3));

            // players
            Assert.AreEqual(options.Roles.Count, game.Players.Length);
            Assert.That(game.Players.All(p => p.Hand.Count == numberOfCardsPerPlayer));
            Assert.That(game.Players.All(p => p.Hand.Cards.All(c => c is not EpidemicCard)));
        }

        private static void AssertEpidemicCardsAreDistributed(Deck<PlayerCard> drawPile, Difficulty difficulty)
        {
            foreach (var chunk in drawPile.Cards.SplitEvenlyInto(PandemicGame.NumberOfEpidemicCards(difficulty)))
            {
                chunk.Count(c => c is EpidemicCard).ShouldBe(1);
            }
        }
    }
}
