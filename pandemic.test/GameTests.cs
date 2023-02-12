using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Events;
using pandemic.GameData;
using pandemic.test.Utils;
using pandemic.Values;
using Shouldly;
using utils;

namespace pandemic.test
{
    internal class GameTests
    {
        [TestCase("Chicago")]
        [TestCase("Washington")]
        public void Drive_or_ferry_player_moves_them_to_city(string toCity)
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, toCity));

            Assert.AreEqual(toCity, game.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Drive_or_ferry_to_garbage_city_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<InvalidActionException>(() =>
                game.Do(new DriveFerryCommand(Role.Medic, "fasdfasdf")));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DriveFerryCommand(Role.Medic, "Beijing")));
        }

        [Test]
        public void Drive_or_ferry_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            AssertEndsTurn(() => game.Do(new DriveFerryCommand(Role.Medic, "Chicago")));
        }

        [Test]
        public void Direct_flight_goes_to_city_and_discards_card()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Miami") });

            (game, _) = game.Do(new DirectFlightCommand(game.CurrentPlayer.Role, "Miami"));

            game.CurrentPlayer.Location.ShouldBe("Miami");
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.CurrentPlayer.Hand.ShouldNotContain(PlayerCards.CityCard("Miami"));
            game.PlayerDiscardPile.TopCard.ShouldBe(PlayerCards.CityCard("Miami"));
        }

        [Test]
        public void Direct_flight_without_card_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Empty });

            Assert.That(
                () => game.Do(new DirectFlightCommand(game.CurrentPlayer.Role, "Miami")),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Direct_flight_throws_when_not_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Miami") });

            Assert.That(
                () => game.Do(new DirectFlightCommand(Role.Scientist, "Miami")),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Direct_flight_to_current_city_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            Assert.That(
                () => game.Do(new DirectFlightCommand(game.CurrentPlayer.Role, "Atlanta")),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Direct_flight_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = PlayerHand.Of("Miami")
            });

            AssertEndsTurn(() => game.Do(new DirectFlightCommand(Role.Medic, "Miami")));
        }

        [Test]
        public void Charter_flight_goes_to_city_and_discards_card()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            (game, _) = game.Do(new CharterFlightCommand(
                game.CurrentPlayer.Role,
                PlayerCards.CityCard("Atlanta"),
                "Bogota"));

            game.CurrentPlayer.Location.ShouldBe("Bogota");
            game.CurrentPlayer.Hand.ShouldNotContain(PlayerCards.CityCard("Atlanta"));
            game.PlayerDiscardPile.TopCard.ShouldBe(PlayerCards.CityCard("Atlanta"));
        }

        [Test]
        public void Charter_flight_to_garbage_city_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<InvalidActionException>(() =>
                game.Do(new CharterFlightCommand(Role.Medic, PlayerCards.CityCard("Atlanta"), "fasdfasdf")));
        }

        [Test]
        public void Charter_flight_without_card_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Empty });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new CharterFlightCommand(Role.Medic, PlayerCards.CityCard("Atlanta"), "Bogota")));
        }

        [Test]
        public void Charter_flight_to_current_location_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new CharterFlightCommand(Role.Medic, PlayerCards.CityCard("Atlanta"), "Atlanta")));
        }

        [Test]
        public void Charter_flight_when_not_turn_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of("Atlanta")
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new CharterFlightCommand(Role.Scientist, PlayerCards.CityCard("Atlanta"), "Bogota")));
        }

        [Test]
        public void Charter_flight_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = PlayerHand.Of("Atlanta")
            });

            AssertEndsTurn(() => game.Do(new CharterFlightCommand(
                Role.Medic,
                PlayerCards.CityCard("Atlanta"),
                "Bogota")));
        }

        [Test]
        public void Shuttle_flight_goes_to_city()
        {
            var game = NewGame(new NewGameOptions
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

            // act
            (game, _) = game.Do(new ShuttleFlightCommand(game.CurrentPlayer.Role, "Bogota"));

            game.CurrentPlayer.Location.ShouldBe("Bogota");
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Shuttle_flight_throws_if_destination_has_no_research_station()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new ShuttleFlightCommand(Role.Medic, "Bogota")));
        }

        [Test]
        public void Shuttle_flight_throws_if_destination_is_current_location()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new ShuttleFlightCommand(Role.Medic, "Atlanta")));
        }

        [Test]
        public void Shuttle_flight_throws_if_location_has_no_research_station()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlanta = game.CityByName("Atlanta");
            var bogota = game.CityByName("Bogota");

            game = game with
            {
                Cities = game.Cities.Replace(atlanta, atlanta with
                {
                    HasResearchStation = false
                }).Replace(bogota, bogota with
                {
                    HasResearchStation = true
                })
            };

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new ShuttleFlightCommand(Role.Medic, "Bogota")));
        }

        [Test]
        public void Shuttle_flight_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var bogota = game.CityByName("Bogota");

            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    ActionsRemaining = 1
                }),
                Cities = game.Cities.Replace(bogota, bogota with
                {
                    HasResearchStation = true
                })
            };

            AssertEndsTurn(() => game.Do(new ShuttleFlightCommand(Role.Medic, "Bogota")));
        }

        [Test]
        public void Shuttle_flight_throws_if_not_turn()
        {
            var game = NewGame(new NewGameOptions
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

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new ShuttleFlightCommand(Role.Scientist, "Bogota")));
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
        {
            var startingState = NewGame(new NewGameOptions { Roles = new[] { Role.Medic, Role.Scientist } })
                .WithNoEpidemics();

            var game = startingState.SetCurrentPlayerAs(
                startingState.CurrentPlayer with { ActionsRemaining = 1 });

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            Assert.AreEqual(
                startingState.PlayerByRole(Role.Medic).Hand.Count + 2,
                game.PlayerByRole(Role.Medic).Hand.Count);
        }

        [Test]
        public void Player_attempts_fifth_action_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Atlanta"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Atlanta"));

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DriveFerryCommand(Role.Medic, "Chicago")));
        }

        [Test]
        public void Cities_are_infected_after_player_turn_ends()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                // epidemics mess with this test, remove them
                PlayerDrawPile = new Deck<PlayerCard>(PlayerCards.CityCards),
                InfectionRateMarkerPosition = 5
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });
            game.InfectionRate.ShouldBe(4);
            var startingState = game;

            var events = new List<IEvent>();

            // act
            game = game.Do(new PassCommand(Role.Medic), events);

            // assert
            game.InfectionDrawPile.Count.ShouldBe(startingState.InfectionDrawPile.Count - game.InfectionRate);
            game.InfectionDiscardPile.Count.ShouldBe(startingState.InfectionDiscardPile.Count + game.InfectionRate);

            foreach (var infectionCard in game.InfectionDiscardPile.Top(game.InfectionRate))
            {
                var city = game.CityByName(infectionCard.City);
                city.Cubes.NumberOf(infectionCard.Colour).ShouldBe(1);
            }

            game.Cubes.Counts().Values.Sum().ShouldBe(startingState.Cubes.Counts().Values.Sum() - game.InfectionRate);
        }

        [Test]
        public void Game_ends_when_cubes_run_out()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                Cubes = CubePile.Empty
            };

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Atlanta"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Atlanta"));

            Assert.IsTrue(game.IsOver);
            Assert.IsTrue(game.IsLost);
        }

        [Test]
        public void It_is_next_players_turn_after_infect_cities()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Atlanta"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Atlanta"));

            game.PlayerByRole(Role.Medic).ActionsRemaining.ShouldBe(4,
                "player whose turn ended should get their 'remaining actions' counter reset");
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
        }

        [Test]
        public void Player_must_discard_when_hand_is_full()
        {
            var game = NewGame(new NewGameOptions { Roles = new[] { Role.Medic, Role.Scientist } })
                .WithNoEpidemics();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(7))
            });

            // act
            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            // assert
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(0);
            new PlayerCommandGenerator().LegalCommands(game).ShouldAllBe(move => move is DiscardPlayerCardCommand);

            foreach (var command in PlayerCommandGenerator.AllPossibleCommands(game).Where(c => c is IConsumesAction))
            {
                Assert.That(
                    () => game.Do(command),
                    Throws.InstanceOf<GameRuleViolatedException>(), $"{command}");
            }
        }

        [Test]
        public void Discard_player_card_goes_to_discard_pile()
        {
            var game = NewGame(new NewGameOptions { Roles = new[] { Role.Medic, Role.Scientist } })
                .WithNoEpidemics();
            var top7Cards = game.PlayerDrawPile.Top(7).ToList();

            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.Remove(top7Cards)
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = new PlayerHand(top7Cards)
            });
            (game, _) = game.Do(new PassCommand(Role.Medic));

            // act
            var cardToDiscard = game.CurrentPlayer.Hand.First();
            (game, _) = game.Do(new DiscardPlayerCardCommand(Role.Medic, cardToDiscard));

            // assert
            game.CurrentPlayer.Hand.CityCards.ShouldNotContain(cardToDiscard);
            game.PlayerDiscardPile.Cards.ShouldContain(cardToDiscard);
        }

        [Test]
        public void Discard_player_card_when_no_actions_infects_cities()
        {
            var initialGame = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var game = initialGame.SetCurrentPlayerAs(initialGame.CurrentPlayer with
            {
                ActionsRemaining = 0,
                Hand = new PlayerHand(initialGame.PlayerDrawPile.Top(8))
            });
            game = game with { PhaseOfTurn = TurnPhase.InfectCities };

            (game, _) = game.Do(new DiscardPlayerCardCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Hand.First()));

            game.InfectionDrawPile.Count.ShouldBe(initialGame.InfectionDrawPile.Count - 2);
            game.InfectionDiscardPile.Count.ShouldBe(initialGame.InfectionDiscardPile.Count + 2);
            TotalNumCubesOnCities(game).ShouldBe(TotalNumCubesOnCities(initialGame) + 2);
        }

        [Test]
        public void Discard_player_card_when_no_actions_and_nine_cards_must_discard_another()
        {
            var initialGame = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var game = initialGame.SetCurrentPlayerAs(initialGame.CurrentPlayer with
            {
                Hand = new PlayerHand(initialGame.PlayerDrawPile.Top(9)),
                ActionsRemaining = 0
            });

            (game, _) = game.Do(new DiscardPlayerCardCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Hand.First()));

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(0);
            new PlayerCommandGenerator().LegalCommands(game).ShouldAllBe(move => move is DiscardPlayerCardCommand);
        }

        [Test]
        public void Discard_player_card_does_not_cause_card_pickup()
        {
            var initialGame = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var game = initialGame.SetCurrentPlayerAs(initialGame.CurrentPlayer with
            {
                Hand = new PlayerHand(initialGame.PlayerDrawPile.Top(9)),
                ActionsRemaining = 0
            });

            (game, var events) = game.Do(new DiscardPlayerCardCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Hand.First()));

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(0);
            events.ShouldNotContain(e => e is PlayerCardPickedUp);
            new PlayerCommandGenerator().LegalCommands(game).ShouldAllBe(move => move is DiscardPlayerCardCommand);
        }

        [Test]
        public void Discard_player_card_throws_when_not_in_hand()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty
            });

            Assert.That(
                () => game.Do(new DiscardPlayerCardCommand(game.CurrentPlayer.Role, PlayerCards.CityCard("Bogota"))),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Scenario_share_knowledge_give_then_other_player_must_discard()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta")
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of("Miami", "New York", "Bogota", "Milan", "Lima", "Paris", "Moscow")
            });

            var commandGenerator = new PlayerCommandGenerator();
            var events = new List<IEvent>();

            var gameStateBeforeShare = game;
            game = game.Do(new ShareKnowledgeGiveCommand(Role.Medic, "Atlanta", Role.Scientist), events);

            commandGenerator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Scientist);

            game = game.Do(new DiscardPlayerCardCommand(Role.Scientist, PlayerCards.CityCard("Miami")), events);

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Scientist).Hand.Count.ShouldBe(7);
            game.PlayerByRole(Role.Scientist).ActionsRemaining.ShouldBe(4);
            game.InfectionDrawPile.Count.ShouldBe(gameStateBeforeShare.InfectionDrawPile.Count);
        }

        [Test]
        public void Scenario_share_knowledge_take_then_other_player_must_discard()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Miami", "New York", "Bogota", "Milan", "Lima", "Paris", "Moscow")
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of("Atlanta")
            });

            var commandGenerator = new PlayerCommandGenerator();
            var events = new List<IEvent>();

            var gameStateBeforeShare = game;
            game = game.Do(new ShareKnowledgeTakeCommand(Role.Medic, "Atlanta", Role.Scientist), events);

            commandGenerator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Medic);

            game = game.Do(new DiscardPlayerCardCommand(Role.Medic, PlayerCards.CityCard("Miami")), events);

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.Count.ShouldBe(7);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Scientist).Hand.Count.ShouldBe(0);
            game.PlayerByRole(Role.Scientist).ActionsRemaining.ShouldBe(4);
            game.InfectionDrawPile.Count.ShouldBe(gameStateBeforeShare.InfectionDrawPile.Count);
        }

        [Test]
        public void Scenario_share_knowledge_at_end_of_turn_when_both_players_hands_are_full()
        {
            var medicHand = PlayerHand.Of("Atlanta", "New York", "Bogota", "Milan", "Lima", "Paris", "Moscow");
            var scientistHand = PlayerHand.Of("Miami", "Taipei", "Sydney", "Delhi", "Jakarta", "Beijing", "Seoul");

            var game = NewGame(new NewGameOptions { Roles = new[] { Role.Medic, Role.Scientist } })
                .WithNoEpidemics();
            game = game with
            {
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards
                    .Where(c => !medicHand.Contains(c) && !scientistHand.Contains(c)))
            };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = medicHand
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = scientistHand
            });

            var cardToShare = PlayerCards.CityCard("Atlanta");
            var commandGenerator = new PlayerCommandGenerator();

            var gameStateBeforeShare = game;
            (game, _) = game.Do(new ShareKnowledgeGiveCommand(Role.Medic, cardToShare.City.Name, Role.Scientist));

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.Count.ShouldBe(6);

            commandGenerator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Scientist);

            (game, _) = game.Do(new DiscardPlayerCardCommand(Role.Scientist, PlayerCards.CityCard("Miami")));

            // medic should now have picked up 2 cards, and needs to discard
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.Count.ShouldBe(8);
            commandGenerator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Medic);
            game.InfectionDrawPile.Count.ShouldBe(gameStateBeforeShare.InfectionDrawPile.Count,
                "infection step should not have occurred yet");

            (game, var lastEvents) = game.Do(new DiscardPlayerCardCommand(Role.Medic, PlayerCards.CityCard("Moscow")));

            lastEvents.ShouldContain(e => e is TurnEnded);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            game.CurrentPlayer.Hand.Count.ShouldBe(7);
            game.CurrentPlayer.Hand.CityCards.ShouldContain(cardToShare);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
            game.PlayerByRole(Role.Medic).Hand.Count.ShouldBe(7);
            game.PlayerByRole(Role.Medic).Hand.CityCards.ShouldNotContain(cardToShare);
            game.InfectionDrawPile.Count.ShouldBe(gameStateBeforeShare.InfectionDrawPile.Count - 2);
        }

        [Test]
        public void Build_research_station_works()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var chicagoPlayerCard = PlayerCards.CityCard("Chicago");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Chicago",
                Hand = PlayerHand.Of(chicagoPlayerCard)
            });

            // act
            (game, _) = game.Do(new BuildResearchStationCommand(game.CurrentPlayer.Role, "Chicago"));

            game.CurrentPlayer.Hand.ShouldNotContain(chicagoPlayerCard);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.CityByName("Chicago").HasResearchStation.ShouldBe(true);
            game.PlayerDiscardPile.TopCard.ShouldBe(chicagoPlayerCard);
            game.ResearchStationPile.ShouldBe(4);
        }

        [Test]
        public void Build_research_station_can_end_turn()
        {
            var game = NewGame(new NewGameOptions { Roles = new[] { Role.Medic, Role.Scientist } })
                .WithNoEpidemics();

            var chicagoPlayerCard = PlayerCards.CityCard("Chicago");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Chicago",
                Hand = PlayerHand.Of(chicagoPlayerCard),
                ActionsRemaining = 1
            });

            AssertEndsTurn(() => game.Do(new BuildResearchStationCommand(game.CurrentPlayer.Role, "Chicago")));
        }

        [Test]
        public void Build_research_station_when_not_in_city_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Chicago")
            });

            Assert.Throws<GameRuleViolatedException>(() => game.Do(
                new BuildResearchStationCommand(game.CurrentPlayer.Role, "Chicago")));
        }

        [Test]
        public void Build_research_station_without_correct_city_card_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty
            });

            Assert.AreEqual("Atlanta", game.CurrentPlayer.Location);
            Assert.Throws<GameRuleViolatedException>(() => game.Do(
                new BuildResearchStationCommand(game.CurrentPlayer.Role, "Atlanta")));
        }

        [Test]
        public void Build_research_station_where_already_exists_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlantaPlayerCard = PlayerCards.CityCard("Atlanta");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of(atlantaPlayerCard)
            });

            // atlanta starts with a research station
            Assert.Throws<GameRuleViolatedException>(() => game.Do(
                new BuildResearchStationCommand(game.CurrentPlayer.Role, "Atlanta")));
        }

        [Test]
        public void Build_research_station_when_none_left_throws()
        {
            var game = NewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist }
                }) with
                {
                    ResearchStationPile = 0
                };

            var chicagoPlayerCard = PlayerCards.CityCard("Chicago");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Chicago",
                Hand = PlayerHand.Of(chicagoPlayerCard)
            });

            Assert.Throws<GameRuleViolatedException>(() => game.Do(
                new BuildResearchStationCommand(game.CurrentPlayer.Role, "Chicago")));
        }

        [Test]
        public void Cure_disease_works()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
            });

            (game, _) = game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));

            Assert.IsTrue(game.IsCured(Colour.Black));
            Assert.AreEqual(0, game.CurrentPlayer.Hand.Count);
            Assert.AreEqual(5, game.PlayerDiscardPile.Count);
            Assert.AreEqual(3, game.CurrentPlayer.ActionsRemaining);
        }

        [Test]
        public void Cure_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5)),
                ActionsRemaining = 1
            });

            AssertEndsTurn(() => game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Cure_last_disease_wins()
        {
            var game = NewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist }
                }) with
                {
                    CuresDiscovered = new List<CureMarker>
                    {
                        new CureMarker(Colour.Blue, CureMarkerSide.Vial),
                        new CureMarker(Colour.Red, CureMarkerSide.Vial),
                        new CureMarker(Colour.Yellow, CureMarkerSide.Vial),
                    }.ToImmutableList()
                };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5)),
            });

            // act
            (game, _) = game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));

            Assert.IsTrue(game.IsWon);
        }

        [Test]
        public void Cure_last_disease_on_last_action_wins()
        {
            var game = NewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist }
                }) with
                {
                    CuresDiscovered = new List<CureMarker>
                    {
                        new CureMarker(Colour.Blue, CureMarkerSide.Vial),
                        new CureMarker(Colour.Red, CureMarkerSide.Vial),
                        new CureMarker(Colour.Yellow, CureMarkerSide.Vial),
                    }.ToImmutableList()
                };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5)),
                ActionsRemaining = 1
            });

            // act
            (game, _) = game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));

            Assert.IsTrue(game.IsWon);
        }

        [Test]
        public void Cure_when_not_at_research_station_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Chicago",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                    game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Cure_when_not_enough_cards_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(4))
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                    game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Cure_with_different_colours_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(new[]
                {
                    new PlayerCityCard(new CityData("asdf", Colour.Black)),
                    new PlayerCityCard(new CityData("asdf", Colour.Black)),
                    new PlayerCityCard(new CityData("asdf", Colour.Black)),
                    new PlayerCityCard(new CityData("asdf", Colour.Black)),
                    new PlayerCityCard(new CityData("asdf", Colour.Blue)),
                })
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                    game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Cure_already_cured_disease_throws()
        {
            var game = NewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist }
                }) with
                {
                    CuresDiscovered = new List<CureMarker>
                    {
                        new CureMarker(Colour.Black, CureMarkerSide.Vial)
                    }.ToImmutableList()
                };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DiscoverCureCommand(game.CurrentPlayer.Role,
                    game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Epidemic_card_goes_to_discard_pile()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(new List<PlayerCard>
                {
                    new EpidemicCard(),
                    new PlayerCityCard(new CityData("asdf", Colour.Black))
                }),
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    ActionsRemaining = 1
                })
            };

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            Assert.IsFalse(game.PlayerByRole(Role.Medic).Hand.Any(c => c is EpidemicCard));
            Assert.AreEqual(1, game.PlayerDiscardPile.Cards.Count(c => c is EpidemicCard));
        }

        [Test]
        public void Treat_disease_works()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var atlanta = game.CityByName("Atlanta");
            game = game with
            {
                Cities = game.Cities.Replace(atlanta, atlanta with { Cubes = CubePile.Empty.AddCube(Colour.Blue) })
            };
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            // act
            (game, _) = game.Do(new TreatDiseaseCommand(game.CurrentPlayer.Role, "Atlanta", Colour.Blue));

            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Treat_disease_throws_if_wrong_city()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var atlanta = game.CityByName("Atlanta");
            var chicago = game.CityByName("Chicago");
            game = game with
            {
                Cities = game.Cities
                    .Replace(atlanta, atlanta with { Cubes = CubePile.Empty.AddCube(Colour.Blue) })
                    .Replace(chicago, chicago with { Cubes = CubePile.Empty.AddCube(Colour.Blue) })
            };

            // act
            Assert.That(
                () => game.Do(new TreatDiseaseCommand(game.CurrentPlayer.Role, "Chicago", Colour.Blue)),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Treat_disease_throws_if_no_cubes()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var atlanta = game.CityByName("Atlanta");
            game = game with
            {
                Cities = game.Cities.Replace(atlanta, atlanta with { Cubes = CubePile.Empty })
            };

            // act
            Assert.That(
                () => game.Do(new TreatDiseaseCommand(game.CurrentPlayer.Role, "Atlanta", Colour.Blue)),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Share_knowledge_give_works()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlanta = PlayerCards.CityCard("Atlanta");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty.Add(atlanta)
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Empty
            });

            // act
            (game, _) = game.Do(new ShareKnowledgeGiveCommand(game.CurrentPlayer.Role, "Atlanta", Role.Scientist));

            game.PlayerByRole(Role.Medic).Hand.ShouldNotContain(atlanta);
            game.PlayerByRole(Role.Medic).ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Scientist).Hand.ShouldContain(atlanta);
        }

        [Test]
        public void Share_knowledge_give_throws_when_receiver_not_in_same_city()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlanta = PlayerCards.CityCard("Atlanta");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty.Add(atlanta)
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Empty,
                Location = "Chicago"
            });

            // act
            Assert.That(
                () => game.Do(new ShareKnowledgeGiveCommand(game.CurrentPlayer.Role, "Atlanta", Role.Scientist)),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Share_knowledge_give_throws_when_not_in_matching_location()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var chicago = PlayerCards.CityCard("Chicago");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty.Add(chicago)
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Empty,
            });

            // act
            Assert.That(
                () => game.Do(new ShareKnowledgeGiveCommand(game.CurrentPlayer.Role, "Chicago", Role.Scientist)),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Share_knowledge_give_throws_when_player_doesnt_have_card()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Empty,
            });

            // act
            Assert.That(
                () => game.Do(new ShareKnowledgeGiveCommand(game.CurrentPlayer.Role, "Atlanta", Role.Scientist)),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Share_knowledge_to_self_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta")
            });

            // act
            Assert.That(
                () => game.Do(new ShareKnowledgeGiveCommand(Role.Medic, "Atlanta", Role.Medic)),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Share_knowledge_receiver_must_discard_if_more_than_7_cards()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta")
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of(PlayerCards.CityCards.Shuffle().Take(7))
            });

            // act
            (game, _) = game.Do(new ShareKnowledgeGiveCommand(game.CurrentPlayer.Role, "Atlanta", Role.Scientist));

            var generator = new PlayerCommandGenerator();
            generator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Scientist);
        }

        [Test]
        public void Share_knowledge_take_works()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlanta = PlayerCards.CityCard("Atlanta");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Empty.Add(atlanta)
            });

            // act
            (game, _) = game.Do(new ShareKnowledgeTakeCommand(game.CurrentPlayer.Role, "Atlanta", Role.Scientist));

            game.PlayerByRole(Role.Medic).Hand.ShouldContain(atlanta);
            game.PlayerByRole(Role.Scientist).Hand.ShouldNotContain(atlanta);
            game.PlayerByRole(Role.Medic).ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Epidemic()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                Rng = new Random(1237)
                // just in case: a seed of 1238 causes failure
            });

            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });
            var initialGame = game;

            (game, var events) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            var epidemicCity = initialGame.InfectionDrawPile.BottomCard;
            game.CityByName(epidemicCity.City).Cubes.NumberOf(epidemicCity.Colour).ShouldBe(3);
            game.InfectionRate.ShouldBe(2);
            game.InfectionDiscardPile.Count.ShouldBe(2);
            game.InfectionDrawPile.Count.ShouldBe(46);
            game.PlayerDiscardPile.Count.ShouldBe(1);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<EpidemicCard>();
        }

        [Test]
        public void Epidemic_increases_infection_rate()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                InfectionRateMarkerPosition = 2,
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, var events) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            game.InfectionRateMarkerPosition.ShouldBe(3);
            game.InfectionRate.ShouldBe(3);
        }

        [Test]
        public void Epidemic_can_end_game()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
                Cubes = CubePile.Empty
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, var events) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            game.IsLost.ShouldBeTrue();
            events.ShouldNotContain(e => e is CubeAddedToCity);
        }

        [Test]
        public void Epidemic_causes_outbreak_scenario()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
                InfectionDiscardPile = Deck<InfectionCard>.Empty,
                InfectionRateMarkerPosition = 5, // ensure that epidemic city is infected immediately
                Cities = game.Cities.Select(c => c with{Cubes = CubePile.Empty}).ToImmutableList() // make sure no additional outbreaks occur
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act
            (game, var events) = game.Do(new PassCommand(Role.Medic));

            // assert
            game.OutbreakCounter.ShouldBe(1);
        }

        [Test]
        public void Epidemic_onto_existing_cubes_causes_outbreak()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
            });

            var epidemicInfectionCard = game.InfectionDrawPile.BottomCard;
            var epidemicCity = game.CityByName(epidemicInfectionCard.City);

            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
                Cities = game.Cities.Replace(epidemicCity, epidemicCity.AddCube(epidemicInfectionCard.Colour))
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act
            (game, var events) = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

            game.CityByName(epidemicInfectionCard.City).Cubes.NumberOf(epidemicInfectionCard.Colour).ShouldBe(3);
            events.ShouldContain(e => e is OutbreakOccurred);
        }

        [Test]
        public void Pass_reduces_num_actions()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            (game, _) = game.Do(new PassCommand(Role.Medic));

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Pass_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, var events) = game.Do(new PassCommand(Role.Medic));

            events.ShouldContain(e => e is TurnEnded);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Eradicate_causes_infection_cards_to_have_no_effect()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                // 1 blue cube on Atlanta, no other cubes
                Cities = game.Cities.Select(c => c.Name switch
                {
                    "Atlanta" => c with { Cubes = CubePile.Empty.AddCube(Colour.Blue) },
                    _ => c with { Cubes = CubePile.Empty }
                }).ToImmutableList(),

                // prevent epidemics
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard)),

                // only blue cards in infection pile
                InfectionDrawPile = new Deck<InfectionCard>(game.InfectionDrawPile.Cards.Where(c => c.Colour == Colour.Blue)),

                // blue cure discovered
                CuresDiscovered = game.CuresDiscovered.Add(new CureMarker(Colour.Blue, CureMarkerSide.Vial))
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act: this eradicates blue
            (game, var events) = game.Do(new TreatDiseaseCommand(Role.Medic, "Atlanta", Colour.Blue));

            // assert
            var eventList = events.ToList();
            eventList.ShouldContain(e => e is DiseaseEradicated);
            game.IsEradicated(Colour.Blue).ShouldBe(true);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            eventList.ShouldContain(e => e is InfectionCardDrawn);
            game.InfectionDiscardPile.Cards.ShouldContain(c => c.Colour == Colour.Blue);
            game.Cities.ShouldNotContain(c => c.Cubes.NumberOf(Colour.Blue) > 0,
                "Expected: blue infection cards have been drawn, but have no effect because blue is eradicated");
        }

        [Test]
        public void Outbreak_infects_adjacent_cities()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
            });

            var atlantaInfectionCard = InfectionCard.FromCity(game.Board.City("Atlanta"));
            game = game with
            {
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard)),
                InfectionDrawPile =
                    game.InfectionDrawPile.Remove(atlantaInfectionCard).PlaceOnTop(atlantaInfectionCard),
                Cities = game.Cities.Select(c => c.Name switch
                {
                    "Atlanta" => c with
                    {
                        Cubes = CubePile.Empty.AddCube(Colour.Blue).AddCube(Colour.Blue).AddCube(Colour.Blue)
                    },
                    _ => c with { Cubes = CubePile.Empty }
                }).ToImmutableList()
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });
            var initialGame = game;

            // act
            (game, _) = game.Do(new PassCommand(Role.Medic));

            game.OutbreakCounter.ShouldBe(initialGame.OutbreakCounter + 1);
            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(3);
            var adjacentCities = game.Board.AdjacentCities["Atlanta"].Select(a => game.CityByName(a));
            adjacentCities.ShouldAllBe(c => c.Cubes.NumberOf(Colour.Blue) >= 1);
        }

        [Test]
        public void Outbreak_x8_causes_loss()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
            });

            var atlanta = game.CityByName("Atlanta");
            game = game with
            {
                OutbreakCounter = 7,
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard)),
                InfectionDrawPile = game.InfectionDrawPile.PlaceOnTop(InfectionCard.FromCity(game.Board.City("Atlanta"))),
                Cities = game.Cities.Replace(atlanta, atlanta with
                {
                    Cubes = CubePile.Empty
                        .AddCube(Colour.Blue)
                        .AddCube(Colour.Blue)
                        .AddCube(Colour.Blue)
                })
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, _) = game.Do(new PassCommand(Role.Medic));

            game.IsLost.ShouldBeTrue();
        }

        [Test]
        public void Outbreak_causes_game_lost_when_cubes_run_out()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
            });

            var atlanta = game.CityByName("Atlanta");
            game = game with
            {
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard)),
                InfectionDrawPile = game.InfectionDrawPile.PlaceOnTop(InfectionCard.FromCity(game.Board.City("Atlanta"))),
                Cities = game.Cities.Replace(atlanta, atlanta with
                {
                    Cubes = CubePile.Empty
                        .AddCube(Colour.Blue)
                        .AddCube(Colour.Blue)
                        .AddCube(Colour.Blue)
                }),
                Cubes = CubePile.Empty.AddCube(Colour.Blue).AddCube(Colour.Blue)
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, _) = game.Do(new PassCommand(Role.Medic));

            game.IsLost.ShouldBeTrue();
        }

        [Test]
        public void Outbreak_scenario_chain_reaction()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
            });

            game = game with
            {
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard)),
                InfectionDrawPile =
                game.InfectionDrawPile.PlaceOnTop(InfectionCard.FromCity(game.Board.City("Atlanta"))),
                Cities = game.Cities.Select(c => c.Name switch
                {
                    "Atlanta" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Blue, 3)
                    },
                    "Chicago" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Blue, 3)
                    },
                    // ensure no cubes on other cities so that there are no more chain reactions
                    _ => c with { Cubes = CubePile.Empty }
                }).ToImmutableList()
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, var events) = game.Do(new PassCommand(Role.Medic));

            game.OutbreakCounter.ShouldBe(2);
            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(3);
            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(3);
            game.Board.AdjacentCities["Atlanta"].Select(a => game.CityByName(a))
                .ShouldAllBe(c => c.Cubes.NumberOf(Colour.Blue) >= 1);
            game.Board.AdjacentCities["Chicago"].Select(a => game.CityByName(a))
                .ShouldAllBe(c => c.Cubes.NumberOf(Colour.Blue) >= 1);
        }

        [Test]
        public void Outbreak_scenario_chain_reaction_big()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
            });

            game = game with
            {
                PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard)),
                InfectionDrawPile =
                    game.InfectionDrawPile.PlaceOnTop(InfectionCard.FromCity(game.Board.City("Beijing"))),
                Cities = game.Cities.Select(c => c.Name switch
                {
                    "Beijing" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Red, 3)
                    },
                    "Osaka" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Red, 3)
                    },
                    "Seoul" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Red, 3)
                    },
                    "Tokyo" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Red, 3)
                    },
                    "Shanghai" => c with
                    {
                        Cubes = CubePile.Empty.AddCubes(Colour.Red, 1)
                    },
                    _ => c with { Cubes = CubePile.Empty }
                }).ToImmutableList()
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act
            (game, var events) = game.Do(new PassCommand(Role.Medic));

            // assert
            game.OutbreakCounter.ShouldBe(5);

            game.CityByName("Beijing").Cubes.NumberOf(Colour.Red).ShouldBe(3);
            game.CityByName("Seoul").Cubes.NumberOf(Colour.Red).ShouldBe(3);
            game.CityByName("Tokyo").Cubes.NumberOf(Colour.Red).ShouldBe(3);
            game.CityByName("Shanghai").Cubes.NumberOf(Colour.Red).ShouldBe(3);
            game.CityByName("Osaka").Cubes.NumberOf(Colour.Red).ShouldBe(3);

            game.Cities.ShouldAllBe(c => ColourExtensions.AllColours.All(
                col => c.Cubes.NumberOf(col) >= 0 && c.Cubes.NumberOf(col) <= 3));
        }

        [Repeat(10)]
        [TestCaseSource(typeof(NewGameOptionsGenerator), nameof(NewGameOptionsGenerator.AllOptions))]
        public void Fuzz_for_invalid_states(NewGameOptions options)
        {
            // bigger numbers here slow down the test, but check for more improper behaviour
            const int illegalCommandsToTryPerTurn = 10;
            var commandGenerator = new PlayerCommandGenerator();
            var random = new Random();
            var (game, events) = PandemicGame.CreateNewGame(options);
            var allPossibleCommands = PlayerCommandGenerator.AllPossibleCommands(game).ToList();

            for (var i = 0; i < 1000 && !game.IsOver; i++)
            {
                var legalCommands = commandGenerator.LegalCommands(game).ToList();

                if (game.Players.Any(p => p.Hand.Count > 7))
                    legalCommands.ShouldAllBe(c => c is DiscardPlayerCardCommand);

                // try a bunch of illegal commands
                foreach (var illegalCommand in allPossibleCommands
                             .Except(legalCommands)
                             .OrderBy(_ => random.Next())
                             .Take(illegalCommandsToTryPerTurn))
                {
                    try
                    {
                        game.Do(illegalCommand);
                        Console.WriteLine(game);
                        Assert.Fail($"Expected {illegalCommand} to throw");
                    }
                    catch (GameRuleViolatedException)
                    {
                        // do nothing: we want an exception thrown!
                    }
                }

                var previousGameState = game;

                // do random action
                var action = random.Choice(commandGenerator.LegalCommands(game));
                (game, var tempEvents) = game.Do(action);
                events.AddRange(tempEvents);

                // check invariants
                var totalCubes = game.Cubes.Counts().Values.Sum() + TotalNumCubesOnCities(game);
                totalCubes.ShouldBe(96);

                var totalPlayerCards = game.Players.Select(p => p.Hand.Count).Sum()
                                       + game.PlayerDrawPile.Count
                                       + game.PlayerDiscardPile.Count;
                totalPlayerCards.ShouldBe(48 + PandemicGame.NumberOfEpidemicCards(game.Difficulty));

                (game.InfectionDrawPile.Count + game.InfectionDiscardPile.Count).ShouldBe(48);

                (game.ResearchStationPile + game.Cities.Count(c => c.HasResearchStation)).ShouldBe(6);

                // todo: fail scenario: epidemic on city that already has 1 cube
                // eg outbreak occured at adjacent city, then this city epidemics
                game.Cities.ShouldAllBe(c => ColourExtensions.AllColours.All(
                    col => c.Cubes.NumberOf(col) >= 0 && c.Cubes.NumberOf(col) <= 3));
            }
        }

        private static int TotalNumCubesOnCities(PandemicGame game)
        {
            return game.Cities.Sum(c => c.Cubes.Counts().Sum(cc => cc.Value));
        }

        private static void AssertEndsTurn(Func<(PandemicGame, IEnumerable<IEvent>)> action)
        {
            IEnumerable<IEvent> events;

            (_, events) = action();

            Assert.IsTrue(events.Any(e => e is TurnEnded));
        }

        private static PandemicGame NewGame(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            return game;
        }
    }
}
