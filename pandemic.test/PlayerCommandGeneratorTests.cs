using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.test
{
    public class PlayerCommandGeneratorTests
    {
        [Test]
        public void Cannot_build_research_station_when_one_already_exists()
        {
            var generator = new PlayerCommandGenerator();
            var game = PandemicGame.CreateUninitialisedGame();
            var atlantaPlayerCard = new PlayerCityCard(game.Board.City("Atlanta"));

            game = game with
            {
                Players = ImmutableList.Create(new Player
                {
                    // atlanta starts with a research station
                    Location = "Atlanta",
                    Hand = new PlayerHand(new[] {atlantaPlayerCard})
                })
            };

            Assert.False(generator.LegalCommands(game).Any(c => c is BuildResearchStationCommand));
        }
    }
}
