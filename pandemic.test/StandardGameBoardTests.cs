using NUnit.Framework;
using pandemic.GameData;

namespace pandemic.test
{
    internal class StandardGameBoardTests
    {
        private readonly StandardGameBoard _board = new();

        [TestCase("Atlanta", "Washington", 1)]
        public void Drive_ferry_distance_between(string city1, string city2, int expectedDistance)
        {
            Assert.That(_board.DriveFerryDistance(city1, city2), Is.EqualTo(expectedDistance));
        }
    }
}
