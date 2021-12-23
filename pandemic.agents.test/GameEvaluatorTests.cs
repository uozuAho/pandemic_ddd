using System.Linq;
using NUnit.Framework;
using pandemic.agents.GreedyBfs;
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

            Assert.AreEqual(expectedScore, GameEvaluator.PlayerHandScore(hand));
        }
    }
}
