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

            Assert.IsTrue(_generator.AllLegalCommands(game).Any(c => c is BuildResearchStationCommand));
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

            Assert.False(_generator.AllLegalCommands(game).Any(c => c is BuildResearchStationCommand));
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

            Assert.IsFalse(_generator.AllLegalCommands(game).Any(c => c is BuildResearchStationCommand));
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

            Assert.IsTrue(_generator.AllLegalCommands(game).Any(c => c is DiscoverCureCommand));
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
                (DiscoverCureCommand) _generator.AllLegalCommands(game).Single(c => c is DiscoverCureCommand);

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

            Assert.IsTrue(_generator.AllLegalCommands(game).Any(c => c is DirectFlightCommand));
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
            var generatedCharterFlightCommands = _generator.AllLegalCommands(game)
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

            _generator.AllLegalCommands(game).ShouldContain(new ShuttleFlightCommand(Role.Medic, "Bogota"));
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

            _generator.AllLegalCommands(game).ShouldContain(new TreatDiseaseCommand(Role.Medic, "Atlanta", Colour.Blue));
        }

        [Test]
        public void Share_give_knowledge()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            _generator.AllLegalCommands(game).ShouldContain(new ShareKnowledgeGiveCommand(Role.Medic, "Atlanta", Role.Scientist));
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

            _generator.AllLegalCommands(game).ShouldContain(new ShareKnowledgeTakeCommand(Role.Medic, "Atlanta", Role.Scientist));
        }

        [Test]
        public void Pass()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            _generator.AllLegalCommands(game).ShouldContain(new PassCommand(Role.Medic));
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

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldContain(c => c is EventForecastCommand, 6*5*4*3*2*1);
        }

        [Test]
        public void Sensible_event_forecast_generates_one_sensible_command()
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
            game = game with
            {
                InfectionDrawPile = new Deck<InfectionCard>(new[]
                {
                    new InfectionCard("Atlanta", Colour.Blue),
                    new InfectionCard("Chicago", Colour.Blue),
                    new InfectionCard("Essen", Colour.Blue),
                    new InfectionCard("London", Colour.Blue),
                    new InfectionCard("Madrid", Colour.Blue),
                    new InfectionCard("Milan", Colour.Blue),
                }),
                // set cities above with cubes in increasing order
                Cities = game.Cities
                    .Replace(game.CityByName("Atlanta"),
                        game.CityByName("Atlanta") with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 1) })
                    .Replace(game.CityByName("Chicago"),
                        game.CityByName("Chicago") with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 1) })
                    .Replace(game.CityByName("Essen"),
                        game.CityByName("Essen") with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 2) })
                    .Replace(game.CityByName("London"),
                        game.CityByName("London") with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 2) })
                    .Replace(game.CityByName("Madrid"),
                        game.CityByName("Madrid") with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 3) })
                    .Replace(game.CityByName("Milan"),
                        game.CityByName("Milan") with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 3) })
            };

            var commands = _generator.AllSensibleCommands(game).ToList();
            commands.ShouldContain(c => c is EventForecastCommand, 1);
            var command = commands.Single(c => c is EventForecastCommand) as EventForecastCommand;

            // later cards are closer to the top of the deck
            command!.Cards.ShouldBe(new []
            {
                new InfectionCard("Madrid", Colour.Blue),
                new InfectionCard("Milan", Colour.Blue),
                new InfectionCard("Essen", Colour.Blue),
                new InfectionCard("London", Colour.Blue),
                new InfectionCard("Atlanta", Colour.Blue),
                new InfectionCard("Chicago", Colour.Blue),
            });
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

            var commands = _generator.AllLegalCommands(game);

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

            var commands = _generator.AllLegalCommands(game);

            commands.Count(c => c is AirliftCommand).ShouldBe(3 * 47); // 3 players, 47 destinations each
        }

        [Test]
        public void Dont_use_special_event_should_not_be_an_option_when_there_are_other_legal_commands()
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

            var commands = _generator.AllLegalCommands(game).ToList();
            commands.ShouldNotContain(c => c is DontUseSpecialEventCommand);
        }

        [Test]
        public void Dont_use_special_event_should_be_an_option_when_all_other_options_are_special_events()
        {
            var game = CreateNewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist },
                    IncludeSpecialEventCards = false
                }) with
                {
                    PhaseOfTurn = TurnPhase.DrawCards
                };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 0,
                Hand = game.CurrentPlayer.Hand.Add(new AirliftCard())
            });

            var commands = _generator.AllLegalCommands(game).ToList();
            commands.ShouldContain(c => c is DontUseSpecialEventCommand);
            commands.ShouldAllBe(c => c is DontUseSpecialEventCommand || c.IsSpecialEvent);
        }

        public static object[] AllSpecialEventCards = SpecialEventCards.All.ToArray();

        [TestCaseSource(nameof(AllSpecialEventCards))]
        public void Special_event_should_not_be_an_option_when_epidemic_is_triggered(PlayerCard card)
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game with { PhaseOfTurn = TurnPhase.Epidemic };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(card)
            });

            var commands = _generator.AllLegalCommands(game).ToList();
            commands.ShouldNotContain(c => c is DontUseSpecialEventCommand);
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

            var commands = _generator.AllLegalCommands(game);
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
            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldNotContain(c => c is DiscardPlayerCardCommand);

            game = game with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 2 };

            // draw cards is done, should now be able to discard
            commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
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
                Cities = game.Cities.Select(c => c with { HasResearchStation = true }).ToImmutableArray()
            };

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldContain(c => c is OperationsExpertBuildResearchStation, 1);
        }

        [Test]
        public void Operations_expert_cannot_build_research_station_if_none_left()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.OperationsExpert, Role.Scientist },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" }) with
            {
                ResearchStationPile = 0
            };

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldNotContain(c => c is OperationsExpertBuildResearchStation);
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

            var commands = _generator.AllLegalCommands(game);
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

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldNotContain(c => c is OperationsExpertDiscardToMoveFromStation);
        }

        [Test]
        public void Researcher_can_give_any_card_to_player_in_same_city()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Researcher, Role.Scientist },
                IncludeSpecialEventCards = false
            });

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldContain(c => c is ResearcherShareKnowledgeGiveCommand, 4);
        }

        [Test]
        public void Player_can_take_any_card_from_researcher_in_same_city()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Scientist, Role.Researcher },
                IncludeSpecialEventCards = false
            });

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldContain(c => c is ShareKnowledgeTakeFromResearcherCommand, 4);
        }

        [Test]
        public void Scientist_can_cure_with_4()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.Scientist, Role.Researcher },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta", "Paris", "Chicago", "Milan")
            });

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldContain(c => c is ScientistDiscoverCureCommand);
        }

        [Test]
        public void Contingency_planner_can_take_event_card()
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.ContingencyPlanner, Role.Researcher },
                IncludeSpecialEventCards = false
            });
            var airlift = new AirliftCard();
            game = game with
            {
                PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(airlift)
            };

            var commands = _generator.AllLegalCommands(game);
            commands.ShouldContain(c => c is ContingencyPlannerTakeEventCardCommand);
        }

        [TestCaseSource(nameof(AllSpecialEventCards))]
        public void Contingency_planner_can_use_event_card(ISpecialEventCard eventCard)
        {
            var game = CreateNewGame(new NewGameOptions
            {
                Roles = new[] { Role.ContingencyPlanner, Role.Researcher },
                IncludeSpecialEventCards = false
            });
            game = game.SetCurrentPlayerAs((ContingencyPlanner)game.CurrentPlayer with
            {
                StoredEventCard = eventCard
            });

            var commands = _generator.AllLegalCommands(game);
            switch (eventCard)
            {
                case AirliftCard: commands.ShouldContain(c => c is AirliftCommand); break;
                case OneQuietNightCard: commands.ShouldContain(c => c is OneQuietNightCommand); break;
                case EventForecastCard: commands.ShouldContain(c => c is EventForecastCommand); break;
                case GovernmentGrantCard: commands.ShouldContain(c => c is GovernmentGrantCommand); break;
                case ResilientPopulationCard: commands.ShouldContain(c => c is ResilientPopulationCommand); break;
                default: Assert.Fail("doh"); break;
            }
        }

        private static PandemicGame CreateNewGame(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            return game;
        }
    }
}
