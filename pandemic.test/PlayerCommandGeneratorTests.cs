using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.GameData;
using pandemic.test.Utils;
using pandemic.Values;
using Shouldly;

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

        [Test]
        public void Can_charter_fly()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            var expectedCharterFlightCommands = game.Cities
                .Select(c => c.Name)
                .Except(new[] { "Atlanta" })
                .Select(cityName => new CharterFlightCommand(game.CurrentPlayer.Role, PlayerCards.CityCard("Atlanta"), cityName))
                .OrderBy(c => c.Destination);

            // act
            var generatedCharterFlightCommands = _generator.LegalCommands(game)
                .Where(c => c is CharterFlightCommand)
                .Cast<CharterFlightCommand>()
                .OrderBy(c => c.Destination);

            CollectionAssert.AreEqual(expectedCharterFlightCommands, generatedCharterFlightCommands);
        }

        [Test]
        public void Can_shuttle_fly()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var bogota = game.CityByName("Bogota");

            game = game with
            {
                Cities = game.Cities.Replace(bogota, bogota with
                {
                    HasResearchStation = true
                })
            };

            _generator.LegalCommands(game).ShouldContain(new ShuttleFlightCommand(Role.Medic, "Bogota"));
        }

        [Test]
        public void Can_treat_disease()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlanta = game.CityByName("Atlanta");

            game = game with
            {
                Cities = game.Cities.Replace(atlanta, atlanta with { Cubes = CubePile.Empty.AddCube(Colour.Blue) })
            };

            _generator.LegalCommands(game).ShouldContain(new TreatDiseaseCommand(Role.Medic, "Atlanta", Colour.Blue));
        }

        [Test]
        public void Can_share_give_knowledge()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            _generator.LegalCommands(game).ShouldContain(new ShareKnowledgeGiveCommand(Role.Medic, "Atlanta", Role.Scientist));
        }

        [Test]
        public void Can_share_take_knowledge()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of("Atlanta")
            });

            _generator.LegalCommands(game).ShouldContain(new ShareKnowledgeTakeCommand(Role.Medic, "Atlanta", Role.Scientist));
        }

        private static PandemicGame CreateNewGame(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            return game;
        }
    }
}
