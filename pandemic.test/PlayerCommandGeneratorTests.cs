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
        private PlayerCommandGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new PlayerCommandGenerator();
        }

        [Test]
        public void Cannot_build_research_station_when_one_already_exists()
        {
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

            Assert.False(_generator.LegalCommands(game).Any(c => c is BuildResearchStationCommand));
        }

        [Test]
        public void Can_cure()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Location = "Atlanta",
                    Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
                })
            };

            Assert.IsTrue(_generator.LegalCommands(game).Any(c => c is CureDiseaseCommand));
        }
    }
}
