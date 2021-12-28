using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class GameEvaluatorTests_HandScore
    {
        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(3, 3)]
        [TestCase(4, 6)]
        [TestCase(5, 10)]
        public void Cards_of_same_colour(int numCards, int expectedScore)
        {
            var board = new StandardGameBoard();

            var hand = new PlayerHand(Enumerable
                .Repeat(new PlayerCityCard(board.City("Atlanta")), numCards));

            var score = GameEvaluator.PlayerHandScore(PandemicGame.CreateUninitialisedGame(), hand);
            Assert.AreEqual(expectedScore, score);
        }

        [Test]
        public void Cards_of_cured_colour_are_worth_zero()
        {
            var board = new StandardGameBoard();
            var game = PandemicGame.CreateUninitialisedGame();
            game = game with
            {
                // blue is cured
                CureDiscovered = ColourExtensions.AllColours
                    .ToImmutableDictionary(c => c, c => c == Colour.Blue)
            };
            var hand = new PlayerHand(Enumerable
                .Repeat(new PlayerCityCard(board.City("Atlanta")), 5));

            // assert
            var score = GameEvaluator.PlayerHandScore(game, hand);
            Assert.AreEqual(0, score);
        }
    }
}
