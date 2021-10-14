using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.test
{
    public class PlayerCommandGeneratorTests
    {
        [Test]
        public void Cannot_build_research_station_when_one_already_exists()
        {
            var generator = new PlayerCommandGenerator();
            var atlantaPlayerCard = new PlayerCityCard(new CityData {Name = "Atlanta"});

            var game = PandemicGame.CreateUninitialisedGame() with
            {
                Players = ImmutableList.Create(new Player
                {
                    // atlanta starts with a research station
                    Location = "Atlanta",
                    Hand = new PlayerHand(new[] {atlantaPlayerCard})
                })
            };

            // todo: rename LegalMoves to legal commands
            Assert.False(generator.LegalMoves(game).Any(c => c is BuildResearchStationCommand));
        }
    }
}
