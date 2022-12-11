using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.Commands;
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
        public void Can_build_research_station()
        {
            var game = PandemicGame.CreateUninitialisedGame();
            var chicagoPlayerCard = new PlayerCityCard(game.Board.City("Chicago"));

            game = game with
            {
                Players = ImmutableList.Create(new Player
                {
                    Location = "Chicago",
                    Hand = new PlayerHand(new[] { chicagoPlayerCard })
                })
            };

            Assert.IsTrue(_generator.LegalCommands(game).Any(c => c is BuildResearchStationCommand));
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
        public void Cannot_build_research_station_if_none_left()
        {
            var game = PandemicGame.CreateUninitialisedGame();
            var chicagoPlayerCard = new PlayerCityCard(game.Board.City("Chicago"));

            game = game with
            {
                ResearchStationPile = 0,
                Players = ImmutableList.Create(new Player
                {
                    Location = "Chicago",
                    Hand = new PlayerHand(new[] { chicagoPlayerCard })
                })
            };

            Assert.IsFalse(_generator.LegalCommands(game).Any(c => c is BuildResearchStationCommand));
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

            Assert.IsTrue(_generator.LegalCommands(game).Any(c => c is DiscoverCureCommand));
        }

        [Test]
        public void Cure_uses_5_cards()
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
                    Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(6))
                })
            };

            var cureCommand =
                (DiscoverCureCommand) _generator.LegalCommands(game).Single(c => c is DiscoverCureCommand);

            Assert.AreEqual(5, cureCommand.Cards.Length);
        }

        [Test]
        public void Can_direct_fly()
        {
            var game = PandemicGame.CreateUninitialisedGame();
            var atlantaCard = PlayerCards.CityCard("Atlanta");

            game = game with
            {
                Players = ImmutableList.Create(new Player
                {
                    Location = "Chicago",
                    Hand = new PlayerHand(new[] { atlantaCard })
                })
            };

            Assert.IsTrue(_generator.LegalCommands(game).Any(c => c is DirectFlightCommand));
        }
    }
}
