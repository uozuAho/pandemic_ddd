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
        public void Build_research_station()
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
        public void Build_research_station_skip_when_one_already_exists()
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
        public void Build_research_station_no_commands_if_none_left()
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
        public void Discover_cure()
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
        public void Discover_cure_uses_5_cards()
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
        public void Direct_fly()
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
        public void Charter_fly()
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
        public void Shuttle_fly()
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
        public void Treat_disease()
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
        public void Share_give_knowledge()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            _generator.LegalCommands(game).ShouldContain(new ShareKnowledgeGiveCommand(Role.Medic, "Atlanta", Role.Scientist));
        }

        [Test]
        public void Share_take_knowledge()
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
        public void Pass()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            _generator.LegalCommands(game).ShouldContain(new PassCommand(Role.Medic));
        }

        [Test]
        public void Event_forecast_generates_all_permutations()
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
        public void Airlift()
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
        public void Airlift_any_time_any_player()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist, Role.Researcher },
                IncludeSpecialEventCards = false
            });
            game = game.SetPlayer(Role.Researcher, game.PlayerByRole(Role.Researcher) with
            {
                Hand = PlayerHand.Of(new AirliftCard())
            });

            var commands = _generator.LegalCommands(game);

            commands.Count(c => c is AirliftCommand).ShouldBe(3 * 47); // 3 players, 47 destinations each
        }

        [Test]
        public void Dont_use_special_event()
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

            commands.ShouldContain(c => c is DontUseSpecialEventCommand);
        }

        [Test]
        public void Special_events_none_when_chosen_not_to_use_them()
        {
            var game = CreateNewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist },
                    IncludeSpecialEventCards = false
                }) with
                {
                    SelfConsistencyCheckingEnabled = false
                };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new AirliftCard())
            });

            (game, _) = game.Do(new DontUseSpecialEventCommand(game.CurrentPlayer.Role));

            var commands = _generator.LegalCommands(game).ToList();
            commands.ShouldNotContain(c => c is DontUseSpecialEventCommand);
            commands.ShouldNotContain(c => c.IsSpecialEvent);
        }

        [Test]
        public void Discard_while_actions_remaining()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(8))
            });

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DiscardPlayerCardCommand, 8);
        }

        [Test]
        public void Discard_before_drawing_cards()
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
        }

        [Test]
        public void Discard_none_while_drawing_cards()
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

            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 1 };

            var commands = _generator.LegalCommands(game);
            commands.ShouldNotContain(c => c is DiscardPlayerCardCommand);

            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 2 };

            // draw cards is done, should now be able to discard
            commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DiscardPlayerCardCommand);
        }

        [Test]
        public void Discard_after_drawing_cards()
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
            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 2 };

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DiscardPlayerCardCommand);
        }

        [Test]
        public void Discard_none_while_epidemic_is_in_progress()
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
        public void Dispatcher_move_pawn_to_pawn_none_if_already_at_destination()
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
        public void Dispatcher_move_any_pawn_to_any_other_pawn()
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

        [Test]
        public void Dispatcher_drive_ferry()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Dispatcher, Role.Scientist },
                IncludeSpecialEventCards = false
            });

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DispatcherDriveFerryPawnCommand, 3); // all neighbours of atlanta
        }

        [Test]
        public void Dispatcher_charter_fly()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Dispatcher, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DispatcherCharterFlyPawnCommand, 47); // all except current city
        }

        [Test]
        public void Dispatcher_direct_fly()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Dispatcher, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta", "Paris", "Moscow", "Sydney")
            });

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DispatcherDirectFlyPawnCommand, 3); // all in hand except current city
        }

        [Test]
        public void Dispatcher_shuttle_fly()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Dispatcher, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game with
            {
                Cities = game.Cities.Select(c => c with { HasResearchStation = true }).ToImmutableList()
            };

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is DispatcherShuttleFlyPawnCommand, 47); // all except current city
        }

        [Test]
        public void Operations_expert_build_research_station()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.OperationsExpert, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" });

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is OperationsExpertBuildResearchStation, 1);
        }

        [Test]
        public void Operations_expert_move_from_research_station()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.OperationsExpert, Role.Scientist },
                IncludeSpecialEventCards = false
            });

            var possibleCommands = game.CurrentPlayer.Hand.Count * 47;

            var commands = _generator.LegalCommands(game);
            commands.ShouldContain(c => c is OperationsExpertDiscardToMoveFromStation, possibleCommands);
        }

        [Test]
        public void Operations_expert_move_from_research_station____not_if_already_done_this_turn()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.OperationsExpert, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            var opex = (OperationsExpert)game.PlayerByRole(Role.OperationsExpert);
            game = game with
            {
                Players = game.Players.Replace(opex, opex with { HasUsedDiscardAndMoveAbilityThisTurn = true })
            };

            var commands = _generator.LegalCommands(game);
            commands.ShouldNotContain(c => c is OperationsExpertDiscardToMoveFromStation);
        }

        private static PandemicGame CreateNewGame(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            return game;
        }
    }
}
