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

        [Test]
        public void Can_pass()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            _generator.LegalCommands(game).ShouldContain(new PassCommand(Role.Medic));
        }

        [Test]
        public void Generates_all_event_forecast_permutations()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new EventForecastCard())
            });

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is EventForecastCommand, 6*5*4*3*2*1);
        }

        [Test]
        public void Can_do_airlift()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new AirliftCard())
            });

            var commands = _generator.LegalCommands(game);

            commands.Count(c => c is AirliftCommand).ShouldBe(2 * 47); // 2 players, 47 destinations each
        }

        [Test]
        public void No_discards_while_drawing_cards()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 0,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(8))
            });

            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 0 };

            // can discard before starting to draw
            // scenario: share knowledge puts another player over hand limit at the end of a turn
            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DiscardPlayerCardCommand);

            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 1 };

            commands = _generator.LegalCommands(game);
            commands.ShouldNotContain(c => c is DiscardPlayerCardCommand);

            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 2 };

            // draw cards is done, should now be able to discard
            commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DiscardPlayerCardCommand);
        }

        [Test]
        public void No_discards_while_epidemic_is_in_progress()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 0,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(8))
            });

            game = game with { PhaseOfTurn = TurnPhase.Epidemic };

            var commands = _generator.LegalCommands(game);
            commands.ShouldNotContain(c => c is DiscardPlayerCardCommand);
        }

        [Test]
        public void Dispatcher_cannot_move_pawn_to_pawn_if_already_at_destination()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Dispatcher, Role.Scientist, Role.QuarantineSpecialist, Role.Medic },
                IncludeSpecialEventCards = false
            });

            var commands = _generator.LegalCommands(game);
            commands.ShouldNotContain(c => c is DispatcherMovePawnToOtherPawnCommand);
        }

        [Test]
        public void Dispatcher_can_move_any_pawn_to_any_other_pawn()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Dispatcher, Role.Scientist, Role.Researcher, Role.Medic },
                IncludeSpecialEventCards = false
            });
            var scientist = game.PlayerByRole(Role.Scientist);
            var researcher = game.PlayerByRole(Role.Researcher);
            var medic = game.PlayerByRole(Role.Medic);
            game = game with
            {
                Players = game.Players
                    .Replace(scientist, scientist with { Location = "Algiers" })
                    .Replace(researcher, researcher with { Location = "Paris" })
                    .Replace(medic, medic with { Location = "Moscow" })
            };

            var commands = _generator.LegalCommands(game);
            // 12 possible commands: each of the 4 players can be moved to 3 other locations
            commands.ShouldContain(c => c is DispatcherMovePawnToOtherPawnCommand, 12);
        }

        private static PandemicGame CreateNewGame(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            return game;
        }
    }
}
