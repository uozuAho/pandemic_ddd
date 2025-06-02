namespace pandemic.test;

using GameData;
using NUnit.Framework;

internal class StandardGameBoardTests
{
    [TestCase("Atlanta", "Washington", 1)]
    [TestCase("Washington", "Atlanta", 1)]
    [TestCase("Atlanta", "New York", 2)]
    [TestCase("Atlanta", "Lima", 3)]
    [TestCase("Santiago", "Paris", 5)]
    public void Drive_ferry_distance_between(string city1, string city2, int expectedDistance)
    {
        Assert.That(
            StandardGameBoard.DriveFerryDistance(city1, city2),
            Is.EqualTo(expectedDistance)
        );
    }
}
