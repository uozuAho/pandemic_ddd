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
            var game = DefaultTestGame();

            (game, _) = game.Do(new DriveFerryCommand(Role.Medic, toCity));

            Assert.AreEqual(toCity, game.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Drive_or_ferry_to_garbage_city_throws()
        {
            var game = DefaultTestGame();

            Assert.Throws<InvalidActionException>(() =>
                game.Do(new DriveFerryCommand(Role.Medic, "fasdfasdf")));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var game = DefaultTestGame();

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new DriveFerryCommand(Role.Medic, "Beijing")));
        }

        [Test]
        public void Drive_or_ferry_can_end_turn()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            AssertEndsTurn(() => game.Do(new DriveFerryCommand(Role.Medic, "Chicago")));
        }

        [Test]
        public void Direct_flight_goes_to_city_and_discards_card()
        {
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Empty });

            Assert.That(
                () => game.Do(new DirectFlightCommand(game.CurrentPlayer.Role, "Miami")),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Direct_flight_throws_when_not_turn()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Miami") });

            Assert.That(
                () => game.Do(new DirectFlightCommand(Role.Scientist, "Miami")),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Direct_flight_to_current_city_throws()
        {
            var game = DefaultTestGame();
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            Assert.That(
                () => game.Do(new DirectFlightCommand(game.CurrentPlayer.Role, "Atlanta")),
                Throws.InstanceOf<GameRuleViolatedException>());
        }

        [Test]
        public void Direct_flight_can_end_turn()
        {
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();

            Assert.Throws<InvalidActionException>(() =>
                game.Do(new CharterFlightCommand(Role.Medic, PlayerCards.CityCard("Atlanta"), "fasdfasdf")));
        }

        [Test]
        public void Charter_flight_without_card_throws()
        {
            var game = DefaultTestGame();
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Empty });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new CharterFlightCommand(Role.Medic, PlayerCards.CityCard("Atlanta"), "Bogota")));
        }

        [Test]
        public void Charter_flight_to_current_location_throws()
        {
            var game = DefaultTestGame();
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new CharterFlightCommand(Role.Medic, PlayerCards.CityCard("Atlanta"), "Atlanta")));
        }

        [Test]
        public void Charter_flight_when_not_turn_throws()
        {
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new ShuttleFlightCommand(Role.Medic, "Bogota")));
        }

        [Test]
        public void Shuttle_flight_throws_if_destination_is_current_location()
        {
            var game = DefaultTestGame();

            Assert.Throws<GameRuleViolatedException>(() =>
                game.Do(new ShuttleFlightCommand(Role.Medic, "Atlanta")));
        }

        [Test]
        public void Shuttle_flight_throws_if_location_has_no_research_station()
        {
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var startingState = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame() with { InfectionRateMarkerPosition = 5 };

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

            game.Cubes.Counts.Values.Sum().ShouldBe(startingState.Cubes.Counts.Values.Sum() - game.InfectionRate);
        }

        [Test]
        public void Game_ends_when_cubes_run_out()
        {
            var game = DefaultTestGame() with
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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

            foreach (var command in AllPlayerCommandGenerator.AllPossibleCommands(game).Where(c => c.ConsumesAction))
            {
                Assert.That(
                    () => game.Do(command),
                    Throws.InstanceOf<GameRuleViolatedException>(), $"{command}");
            }
        }

        [Test]
        public void Discard_throws_when_still_drawing_cards()
        {
            var game = DefaultTestGame() with { PhaseOfTurn = TurnPhase.DrawCards, CardsDrawn = 1 };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 0,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(8))
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DiscardPlayerCardCommand(Role.Medic, game.CurrentPlayer.Hand.First())));
        }

        [Test]
        public void Discard_player_card_goes_to_discard_pile()
        {
            var game = DefaultTestGame();
            var top7Cards = game.PlayerDrawPile.Top(7).ToList();

            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.RemoveIfPresent(top7Cards)
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
            var initialGame = DefaultTestGame();
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
            var initialGame = DefaultTestGame();
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
            var initialGame = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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

            var game = DefaultTestGame();
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
            var events = new List<IEvent>();

            game = game.Do(new ShareKnowledgeGiveCommand(Role.Medic, cardToShare.City.Name, Role.Scientist), events);

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.Count.ShouldBe(6);

            commandGenerator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Scientist);

            game = game.Do(new DiscardPlayerCardCommand(Role.Scientist, PlayerCards.CityCard("Miami")), events);

            // medic should now have picked up 2 cards, and needs to discard
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.Count.ShouldBe(8);
            commandGenerator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Medic);
            game.InfectionDrawPile.Count.ShouldBe(gameStateBeforeShare.InfectionDrawPile.Count,
                "infection step should not have occurred yet");

            game = game.Do(new DiscardPlayerCardCommand(Role.Medic, PlayerCards.CityCard("Moscow")), events);

            events.ShouldContain(e => e is TurnEnded);
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame() with
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame()
                .Cure(Colour.Blue)
                .Cure(Colour.Red)
                .Cure(Colour.Yellow);

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
            var game = DefaultTestGame() with
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame() with
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
            var game = DefaultTestGame();

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
        public void Epidemic_after_7_cards_in_hand()
        {
            var game = DefaultTestGame();

            var drawPile = new Deck<PlayerCard>(PlayerCards.CityCards);
            var playerHand = drawPile.Top(7).ToList();
            drawPile = drawPile.Remove(playerHand);
            drawPile = drawPile
                .Remove(drawPile.TopCard)
                .PlaceOnTop(drawPile.TopCard, new EpidemicCard());

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = drawPile,
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    ActionsRemaining = 1,
                    Hand = new PlayerHand(playerHand)
                }),
            };

            var events = new List<IEvent>();

            // act: do last action, pick up 2 cards
            game = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"), events);

            // assert: epidemic has occurred
            events.ShouldContain(e => e is PlayerCardPickedUp, 2);
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldContain(e => e is EpidemicIntensified);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is EpidemicCard);

            // assert: need to discard
            game.PlayerByRole(Role.Medic).Hand.Count.ShouldBe(8);
            game.APlayerMustDiscard.ShouldBeTrue();

            // act: discard
            game = game.Do(
                new DiscardPlayerCardCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Hand.CityCards.First()),
                events);

            // assert: turn is over
            events.ShouldContain(e => e is TurnEnded);
            game.PlayerByRole(Role.Medic).Hand.Count.ShouldBe(7);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Epidemic_after_7_cards_in_hand__epidemic_is_second_card()
        {
            var game = DefaultTestGame();

            var drawPile = new Deck<PlayerCard>(PlayerCards.CityCards);
            var playerHand = drawPile.Top(7).ToList();
            drawPile = drawPile.Remove(playerHand);
            drawPile = drawPile
                .Remove(drawPile.TopCard)
                .PlaceOnTop(new EpidemicCard(), drawPile.TopCard);

            game = game with
            {
                PlayerDrawPile = drawPile,
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    ActionsRemaining = 1,
                    Hand = new PlayerHand(playerHand)
                })
            };

            var events = new List<IEvent>();

            // act: do last action, pick up 2 cards
            game = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"), events);

            // assert: epidemic has occurred
            events.ShouldContain(e => e is PlayerCardPickedUp, 2);
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldContain(e => e is EpidemicIntensified);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is EpidemicCard);

            // assert: need to discard
            game.PlayerByRole(Role.Medic).Hand.Count.ShouldBe(8);
            game.APlayerMustDiscard.ShouldBeTrue();

            // act: discard
            game = game.Do(
                new DiscardPlayerCardCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Hand.CityCards.First()),
                events);

            // assert: turn is over
            events.ShouldContain(e => e is TurnEnded);
            game.PlayerByRole(Role.Medic).Hand.Count.ShouldBe(7);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Epidemic_after_6_cards_incl_event_in_hand_should_not_need_to_discard()
        {
            var game = DefaultTestGame();

            var drawPile = new Deck<PlayerCard>(PlayerCards.CityCards);
            var playerHand = drawPile.Top(5).Concat(new[] {new ResilientPopulationCard()}).ToList();
            drawPile = drawPile.RemoveIfPresent(playerHand);
            drawPile = drawPile
                .Remove(drawPile.TopCard)
                .PlaceOnTop(new EpidemicCard(), drawPile.TopCard);

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = drawPile,
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = new PlayerHand(playerHand)
                }),
            };

            var events = new List<IEvent>();

            // act: do last action, pick up 2 cards
            game = game.Do(new PassCommand(game.CurrentPlayer.Role), events);

            // assert: epidemic has occurred, paused before intensify
            events.ShouldContain(e => e is PlayerCardPickedUp, 2);
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldNotContain(e => e is EpidemicIntensified);
            events.ShouldNotContain(e => e is TurnEnded);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is EpidemicCard);
            game.APlayerMustDiscard.ShouldBeFalse();
            game.LegalCommands().ShouldContain(c => c is ResilientPopulationCommand);
        }

        [Test]
        public void Treat_disease_works()
        {
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

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
        public void Epidemic_allows_special_event_after_pass_ends_turn()
        {
            var game = DefaultTestGame();
            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(new GovernmentGrantCard())
            });
            var events = new List<IEvent>();

            // act
            game = game.Do(new PassCommand(Role.Medic), events);

            // assert
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldNotContain(e => e is EpidemicIntensified);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            new PlayerCommandGenerator().LegalCommands(game).ShouldContain(c => c is GovernmentGrantCommand);
        }

        [Test]
        public void Epidemic_increases_infection_rate()
        {
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
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
            var game = DefaultTestGame();
            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
                InfectionDiscardPile = Deck<InfectionCard>.Empty,
                InfectionRateMarkerPosition = 5, // ensure that epidemic city is infected immediately
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act
            (game, var events) = game.Do(new PassCommand(Role.Medic));

            // assert
            game.OutbreakCounter.ShouldBe(1);
        }

        [Test]
        public void Epidemic_onto_existing_cube_causes_outbreak()
        {
            var game = DefaultTestGame();
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
        public void Epidemic_onto_multiple_existing_cubes_causes_outbreak()
        {
            // set the rng to prevent the epidemic city ending up on the top of the
            // infection pile after the epidemic
            var game = DefaultTestGame(DefaultTestGameOptions() with {Rng = new Random(1234)});
            var epidemicInfectionCard = game.InfectionDrawPile.BottomCard;
            var epidemicCity = game.CityByName(epidemicInfectionCard.City);

            game = game
                    .RemoveAllCubesFromCities()
                    .AddCube(epidemicCity.Name, epidemicInfectionCard.Colour)
                    .AddCube(epidemicCity.Name, epidemicInfectionCard.Colour) with
                {
                    PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                        PlayerCards.CityCard("Atlanta"),
                        new EpidemicCard()),
                };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act
            (game, var events) = game.Do(new PassCommand(Role.Medic));

            game.CityByName(epidemicInfectionCard.City).Cubes.NumberOf(epidemicInfectionCard.Colour).ShouldBe(3);
            events.ShouldContain(e => e is OutbreakOccurred, 1);
        }

        [Test]
        public void Epidemic_double()
        {
            var game = DefaultTestGame();
            var epidemicInfectionCards = game.InfectionDrawPile.Bottom(2).ToList();
            var epidemicInfectionCard1 = epidemicInfectionCards[0];
            var epidemicInfectionCard2 = epidemicInfectionCards[1];

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    new EpidemicCard(),
                    new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            var events = new List<IEvent>();

            // act
            game = game.Do(new PassCommand(Role.Medic), events);

            game.InfectionRateMarkerPosition.ShouldBe(2);
            game.CityByName(epidemicInfectionCard1.City).Cubes.NumberOf(epidemicInfectionCard1.Colour).ShouldBe(3);
            game.CityByName(epidemicInfectionCard2.City).Cubes.NumberOf(epidemicInfectionCard2.Colour).ShouldBe(3);

            // city 2 was the only card in the infection discard pile on the second epidemic, so should outbreak
            // during the infection stage
            events.ShouldContain(new OutbreakOccurred(epidemicInfectionCard2.City));

            events.ShouldContain(e => e is InfectionCardDrawn, 2);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Scenario_epidemic_double_prevent_outbreak_with_resilient_population()
        {
            var game = DefaultTestGame();
            var epidemicInfectionCards = game.InfectionDrawPile.Bottom(2).ToList();
            var epidemicInfectionCard1 = epidemicInfectionCards[0];
            var epidemicInfectionCard2 = epidemicInfectionCards[1];

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    new EpidemicCard(),
                    new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(new ResilientPopulationCard())
            });

            var events = new List<IEvent>();

            // act: end turn
            game = game.Do(new PassCommand(Role.Medic), events);

            // assert: current state should be: first epidemic, just after infect step,
            // chance to use resilient population card
            events.ShouldContain(e => e is EpidemicInfectStepCompleted, 1);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);

            // act: don't use it just yet
            game = game.Do(new DontUseSpecialEventCommand(game.CurrentPlayer.Role), events);

            // assert: current state should be second epidemic, just after infect step
            events.ShouldContain(e => e is EpidemicInfectStepCompleted, 2);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);

            // act: use resilient population
            game = game.Do(new ResilientPopulationCommand(Role.Medic, epidemicInfectionCard2), events);

            game.InfectionRateMarkerPosition.ShouldBe(2);
            game.CityByName(epidemicInfectionCard1.City).Cubes.NumberOf(epidemicInfectionCard1.Colour).ShouldBe(3);
            game.CityByName(epidemicInfectionCard2.City).Cubes.NumberOf(epidemicInfectionCard2.Colour).ShouldBe(3);

            // city 2 was the only card in the infection discard pile on the second epidemic,
            // so an outbreak would have occurred here if the resilient population card wasn't used
            events.ShouldNotContain(new OutbreakOccurred(epidemicInfectionCard2.City));

            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Pass_ends_turn()
        {
            var game = DefaultTestGame().WithNoEpidemics();

            (game, var events) = game.Do(new PassCommand(Role.Medic));

            events.ShouldContain(e => e is TurnEnded);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Pass_skips_special_event()
        {
            var game = DefaultTestGame();
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(new GovernmentGrantCard())
            });

            (game, var events) = game.Do(new PassCommand(Role.Medic));

            events.ShouldContain(e => e is TurnEnded);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Eradicate_causes_infection_cards_to_have_no_effect()
        {
            var game = DefaultTestGame();
            game = game
                .RemoveAllCubesFromCities()
                .AddCube("Atlanta", Colour.Blue)
                .Cure(Colour.Blue) with
            {
                // only blue cards in infection pile
                InfectionDrawPile = new Deck<InfectionCard>(game.InfectionDrawPile.Cards.Where(c => c.Colour == Colour.Blue)),
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
            var game = DefaultTestGame();

            var atlantaInfectionCard = InfectionCard.FromCity(game.Board.City("Atlanta"));
            game = game
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3) with
            {
                InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlantaInfectionCard)
                        .PlaceOnTop(atlantaInfectionCard),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            // act
            (game, _) = game.Do(new PassCommand(Role.Medic));

            game.OutbreakCounter.ShouldBe(1);
            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(3);
            var adjacentCities = game.Board.AdjacentCities["Atlanta"].Select(a => game.CityByName(a));
            adjacentCities.ShouldAllBe(c => c.Cubes.NumberOf(Colour.Blue) >= 1);
        }

        [Test]
        public void Outbreak_x8_causes_loss()
        {
            var game = DefaultTestGame();

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
            var game = DefaultTestGame();

            var atlanta = game.CityByName("Atlanta");
            var atlantaInfectionCard = new InfectionCard("Atlanta", Colour.Blue);
            game = game with
            {
                InfectionDrawPile = game.InfectionDrawPile
                    .RemoveIfPresent(atlantaInfectionCard)
                    .PlaceOnTop(atlantaInfectionCard),
                Cities = game.Cities.Replace(atlanta, atlanta with
                {
                    Cubes = CubePile.Empty.AddCubes(Colour.Blue, 3)
                }),
                Cubes = CubePile.Empty.AddCubes(Colour.Blue, 2)
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, var events) = game.Do(new PassCommand(Role.Medic));

            game.IsLost.ShouldBeTrue();
        }

        [Test]
        public void Outbreak_scenario_chain_reaction()
        {
            var game = DefaultTestGame();

            var atlanta = new InfectionCard("Atlanta", Colour.Blue);
            var chicago = new InfectionCard("Chicago", Colour.Blue);
            game = game
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3)
                    .AddCubes("Chicago", Colour.Blue, 3) with
                {
                    InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlanta).PlaceOnTop(atlanta)
                        .RemoveIfPresent(chicago),
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
            var game = DefaultTestGame();

            var beijing = new InfectionCard("Beijing", Colour.Red);
            var atlanta = new InfectionCard("Atlanta", Colour.Blue);
            game = game
                    .RemoveAllCubesFromCities()
                    .AddCubes("Beijing", Colour.Red, 3)
                    .AddCubes("Osaka", Colour.Red, 3)
                    .AddCubes("Seoul", Colour.Red, 3)
                    .AddCubes("Tokyo", Colour.Red, 3)
                    .AddCubes("Shanghai", Colour.Red, 1) with
                {
                    InfectionDrawPile = game.InfectionDrawPile
                        .RemoveIfPresent(atlanta).PlaceOnTop(atlanta)
                        .RemoveIfPresent(beijing).PlaceOnTop(beijing),
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

        [Test]
        public void Government_grant_builds_station_without_using_action()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new GovernmentGrantCard())
            });

            new PlayerCommandGenerator()
                .LegalCommands(game)
                .ShouldContain(c => c is GovernmentGrantCommand, 47, "one for each city except Atlanta");

            (game, _) = game.Do(new GovernmentGrantCommand(Role.Medic, "Chicago"));

            game.PlayerByRole(Role.Medic).ActionsRemaining.ShouldBe(4);
            game.CityByName("Chicago").HasResearchStation.ShouldBeTrue();
            game.ResearchStationPile.ShouldBe(4);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<GovernmentGrantCard>();
        }

        [Test]
        public void Government_grant_on_existing_research_station_throws()
        {
            var game = DefaultTestGame();

            var chicago = game.CityByName("Chicago");
            game = game with { Cities = game.Cities.Replace(chicago, chicago with { HasResearchStation = true }) };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new GovernmentGrantCard())
            });

            Should.Throw<GameRuleViolatedException>(() => game.Do(new GovernmentGrantCommand(Role.Medic, "Chicago")));
        }

        [Test]
        public void Government_grant_when_player_doesnt_have_card_throws()
        {
            var game = DefaultTestGame();

            Should.Throw<GameRuleViolatedException>(() => game.Do(new GovernmentGrantCommand(Role.Medic, "Chicago")));
        }

        [Test]
        public void Government_grant_when_no_stations_left_throws()
        {
            var game = DefaultTestGame() with { ResearchStationPile = 0 };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new GovernmentGrantCard())
            });

            Should.Throw<GameRuleViolatedException>(() => game.Do(new GovernmentGrantCommand(Role.Medic, "Chicago")));
        }

        [Test]
        public void Government_grant_can_play_when_not_your_turn()
        {
            var game = DefaultTestGame();
            var scientist = game.PlayerByRole(Role.Scientist);
            game = game.SetPlayer(Role.Scientist,
                scientist with { Hand = scientist.Hand.Add(new GovernmentGrantCard()) });

            new PlayerCommandGenerator()
                .LegalCommands(game)
                .ShouldContain(c => c is GovernmentGrantCommand, 47, "one for each city except Atlanta");

            (game, _) = game.Do(new GovernmentGrantCommand(Role.Scientist, "Chicago"));

            game.CityByName("Chicago").HasResearchStation.ShouldBeTrue();
            game.PlayerByRole(Role.Medic).ActionsRemaining.ShouldBe(4);
            game.PlayerByRole(Role.Scientist).ActionsRemaining.ShouldBe(4);
        }

        [Test]
        public void Government_grant_can_use_instead_of_discarding()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = new PlayerHand(game.PlayerDrawPile.Bottom(5).Concat(new[] { new GovernmentGrantCard() })),
            });

            // act: end turn
            var eventList = new List<IEvent>();
            game = game.Do(new PassCommand(Role.Medic), eventList);

            // assert
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.Hand.Count.ShouldBe(8);
            new PlayerCommandGenerator().LegalCommands(game).ShouldContain(c => c is GovernmentGrantCommand);

            // act: gov grant
            game = game.Do(new GovernmentGrantCommand(Role.Medic, "Chicago"), eventList);

            // assert
            game.CityByName("Chicago").HasResearchStation.ShouldBeTrue();
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<GovernmentGrantCard>();
            game.PlayerByRole(Role.Medic).Hand.Count.ShouldBe(7);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
        }

        [Test]
        public void Government_grant_can_play_during_epidemic_after_infect()
        {
            var game = DefaultTestGame();

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(new GovernmentGrantCard())
            });
            var epidemicInfectionCard = game.InfectionDrawPile.BottomCard;

            var generator = new PlayerCommandGenerator();
            var events = new List<IEvent>();

            // act: end turn, draw epidemic card
            game = game.Do(new PassCommand(Role.Medic), events);

            // assert: epidemic card drawn, infect stage of epidemic has occurred
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldNotContain(e => e is EpidemicIntensified);
            events.ShouldNotContain(e => e is InfectionCardDrawn);
            var epidemicCity = game.CityByName(epidemicInfectionCard.City);
            epidemicCity.Cubes.NumberOf(epidemicInfectionCard.Colour).ShouldBe(3);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            generator.LegalCommands(game).ShouldContain(c => c is GovernmentGrantCommand);

            // act: use special event card
            game = game.Do(new GovernmentGrantCommand(Role.Medic, "Chicago"), events);

            // assert: intensify stage of epidemic has occurred, turn is now over
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            events.ShouldContain(e => e is EpidemicIntensified);
            events.ShouldContain(e => e is InfectionCardDrawn);
        }

        public static object[] AllSpecialEventCards = SpecialEventCards.All.ToArray();

        public static object[] AllSpecials_EpidemicFirst = SpecialEventCards.All.Select(card => new object[] { card, true }).ToArray();
        public static object[] AllSpecials_EpidemicSecond = SpecialEventCards.All.Select(card => new object[] { card, false }).ToArray();

        [TestCaseSource(nameof(AllSpecials_EpidemicFirst))]
        [TestCaseSource(nameof(AllSpecials_EpidemicSecond))]
        public void Dont_use_special_event_during_epidemic(PlayerCard eventCard, bool epidemicCardIsFirst)
        {
            var game = DefaultTestGame();

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = epidemicCardIsFirst
                    ? game.PlayerDrawPile.PlaceOnTop(new EpidemicCard())
                    : game.PlayerDrawPile
                        .Remove(game.PlayerDrawPile.TopCard)
                        .PlaceOnTop(new EpidemicCard(), game.PlayerDrawPile.TopCard)
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(eventCard)
            });

            var events = new List<IEvent>();

            // act: end turn, draw epidemic card
            game = game.Do(new PassCommand(Role.Medic), events);

            // assert: epidemic infection occurred, chance to use special event before intensify
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            events.ShouldContain(e => e is PlayerCardPickedUp);
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldNotContain(e => e is EpidemicIntensified);
            game.LegalCommands().ShouldContain(c => c.IsSpecialEvent);

            // act: don't use special event
            game = game.Do(new DontUseSpecialEventCommand(game.CurrentPlayer.Role), events);

            // assert: epidemic intensified, 2 cards picked up, turn over
            events.ShouldContain(e => e is EpidemicIntensified);
            events.ShouldContain(e => e is PlayerCardPickedUp, 2);
            events.ShouldContain(e => e is InfectionCardDrawn, 2);
            events.ShouldContain(e => e is TurnEnded);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            game.LegalCommands().ShouldContain(c => c.IsSpecialEvent);
        }

        // 'Don't use event' makes no sense when there's other possible actions
        [TestCaseSource(nameof(AllSpecialEventCards))]
        public void Dont_use_special_during_turn_should_throw(PlayerCard eventCard)
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(eventCard)
            });

            Should.Throw<GameRuleViolatedException>(() => game.Do(new DontUseSpecialEventCommand(Role.Medic)));
        }

        [TestCaseSource(nameof(AllSpecialEventCards))]
        public void Dont_use_special_event_after_turn(PlayerCard eventCard)
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(eventCard)
            });

            var generator = new PlayerCommandGenerator();
            var events = new List<IEvent>();

            // act
            game = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"), events);

            // assert: still a chance to use special event before picking up cards
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            events.ShouldNotContain(e => e is PlayerCardPickedUp);
            generator.LegalCommands(game).ShouldContain(c => c.IsSpecialEvent);

            // act: don't use special event
            game = game.Do(new DontUseSpecialEventCommand(game.CurrentPlayer.Role), events);

            // assert: turn ended
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            events.ShouldContain(e => e is PlayerCardPickedUp, 2);
            events.ShouldContain(e => e is InfectionCardDrawn, 2);
            generator.LegalCommands(game).ShouldContain(c => c.IsSpecialEvent);
        }

        [TestCaseSource(nameof(AllSpecialEventCards))]
        public void Dont_use_special_event_after_turn_when_other_player_has_event_card(PlayerCard eventCard)
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
            });
            game = game.SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = game.PlayerByRole(Role.Scientist).Hand.Add(eventCard)
            });

            var generator = new PlayerCommandGenerator();
            var events = new List<IEvent>();

            // act
            game = game.Do(new DriveFerryCommand(Role.Medic, "Chicago"), events);

            // assert: scientist still a chance to use special event before medic picks up cards
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            events.ShouldNotContain(e => e is PlayerCardPickedUp);
            generator.LegalCommands(game).ShouldContain(c => c.IsSpecialEvent);

            // act: don't use special event
            game = game.Do(new DontUseSpecialEventCommand(game.CurrentPlayer.Role), events);

            // assert
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            events.ShouldContain(e => e is PlayerCardPickedUp, 2);
            events.ShouldContain(e => e is InfectionCardDrawn, 2);
            generator.LegalCommands(game).ShouldContain(c => c.IsSpecialEvent);
        }

        [Test]
        public void Event_forecast_happy_path()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new EventForecastCard())
            });

            var top6InfectionCards = new Deck<InfectionCard>(game.InfectionDrawPile.Top(6));
            var newInfectionCardOrder = top6InfectionCards
                .Remove(top6InfectionCards.TopCard)
                .PlaceAtBottom(top6InfectionCards.TopCard)
                .Cards.ToImmutableList();

            (game, var events) = game.Do(new EventForecastCommand(Role.Medic, newInfectionCardOrder));

            game.InfectionDrawPile.Top(6).ShouldBe(newInfectionCardOrder);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is EventForecastCard);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<EventForecastCard>();
        }

        [Test]
        public void Event_forecast_throws_if_not_in_hand()
        {
            var game = DefaultTestGame();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new EventForecastCommand(Role.Medic, ImmutableList<InfectionCard>.Empty)));
        }

        [Test]
        public void Event_forecast_throws_if_cards_are_not_at_top_of_infection_deck()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new EventForecastCard())
            });

            var cardsToReorder = game.InfectionDrawPile.Top(5)
                .Concat(game.InfectionDrawPile.Bottom(1)).ToImmutableList();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new EventForecastCommand(game.CurrentPlayer.Role, cardsToReorder)));
        }

        [Test]
        public void Airlift_happy_path()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new AirliftCard())
            });

            (game, var events) = game.Do(new AirliftCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Role, "Paris"));

            game.CurrentPlayer.Location.ShouldBe("Paris");
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is AirliftCard);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<AirliftCard>();
        }

        [Test]
        public void Airlift_can_move_any_pawn()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new AirliftCard())
            });
            var otherPlayer = Role.Scientist;

            (game, var events) = game.Do(new AirliftCommand(game.CurrentPlayer.Role, otherPlayer, "Paris"));

            game.PlayerByRole(otherPlayer).Location.ShouldBe("Paris");
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is AirliftCard);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<AirliftCard>();
        }

        [Test]
        public void Airlift_throws_if_not_in_hand()
        {
            var game = DefaultTestGame();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new AirliftCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Role, "Paris")));
        }

        [Test]
        public void Airlift_throws_if_already_at_destination()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new AirliftCard())
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new AirliftCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Role, "Atlanta")));
        }

        [Test]
        public void Resilient_population_happy_path()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new ResilientPopulationCard())
            });

            var infectionCardToRemove = game.InfectionDiscardPile.TopCard;

            (game, var events) = game.Do(new ResilientPopulationCommand(game.CurrentPlayer.Role, infectionCardToRemove));

            game.InfectionDiscardPile.Cards.ShouldNotContain(infectionCardToRemove);
            game.InfectionDrawPile.Cards.ShouldNotContain(infectionCardToRemove);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
            game.CurrentPlayer.Hand.ShouldNotContain(c => c is ResilientPopulationCard);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<ResilientPopulationCard>();
        }

        [Test]
        public void Resilient_population_throws_if_not_in_hand()
        {
            var game = DefaultTestGame();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ResilientPopulationCommand(game.CurrentPlayer.Role, game.InfectionDiscardPile.TopCard)));
        }

        [Test]
        public void Resilient_population_throws_if_infection_card_is_not_in_discard_pile()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new ResilientPopulationCard())
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ResilientPopulationCommand(game.CurrentPlayer.Role, game.InfectionDrawPile.TopCard)));
        }

        [Test]
        public void Resilient_population_can_play_during_epidemic_after_infect()
        {
            var game = DefaultTestGame().WithNoEpidemics();

            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(new EpidemicCard()),
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = game.CurrentPlayer.Hand.Add(new ResilientPopulationCard())
            });

            var events = new List<IEvent>();

            // act: end turn, draw epidemic card
            game = game.Do(new PassCommand(Role.Medic), events);

            // assert: infect stage of epidemic has occurred
            events.ShouldContain(e => e is EpidemicTriggered);
            events.ShouldContain(e => e is EpidemicInfectStepCompleted);
            events.ShouldNotContain(e => e is EpidemicIntensified);
            events.ShouldNotContain(e => e is InfectionCardDrawn);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);

            // act: use special event card
            var infectionCardToRemove = game.InfectionDiscardPile.TopCard;
            game = game.Do(new ResilientPopulationCommand(Role.Medic, infectionCardToRemove), events);

            // assert: turn is over, infection card is out of the game
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            events.ShouldContain(e => e is EpidemicIntensified);
            events.ShouldContain(e => e is InfectionCardDrawn);
            game.InfectionDiscardPile.Cards.ShouldNotContain(infectionCardToRemove);
            game.InfectionDrawPile.Cards.ShouldNotContain(infectionCardToRemove);
        }

        [Test]
        public void One_quiet_night_happy_path()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new OneQuietNightCard())
            });
            var initial = game;
            var playerWithCard = game.CurrentPlayer.Role;

            var events = new List<IEvent>();
            game = game.Do(new OneQuietNightCommand(game.CurrentPlayer.Role), events);
            game = game.Do(new PassCommand(game.CurrentPlayer.Role), events);

            events.ShouldNotContain(e => e is InfectionCardDrawn);
            game.InfectionDiscardPile.ShouldBe(initial.InfectionDiscardPile);
            game.InfectionDrawPile.ShouldBe(initial.InfectionDrawPile);
            game.PlayerByRole(playerWithCard).Hand.ShouldNotContain(c => c is OneQuietNightCard);
            game.PlayerDiscardPile.TopCard.ShouldBeOfType<OneQuietNightCard>();
        }

        [Test]
        public void One_quiet_night_only_skips_one_infect_phase()
        {
            var game = DefaultTestGame();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(new OneQuietNightCard())
            });

            var events = new List<IEvent>();

            // act
            game = game.Do(new OneQuietNightCommand(game.CurrentPlayer.Role), events);
            game = game.Do(new PassCommand(game.CurrentPlayer.Role), events);

            // assert: no infection, turn over
            events.ShouldNotContain(e => e is InfectionCardDrawn);
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);

            // act: pass turn
            game = game.Do(new PassCommand(game.CurrentPlayer.Role), events);

            // assert: infection occurred, turn over
            events.ShouldContain(e => e is InfectionCardDrawn);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
        }

        [Test]
        public void One_quiet_night_throws_if_not_in_hand()
        {
            var game = DefaultTestGame();

            Should.Throw<GameRuleViolatedException>(() => game.Do(new OneQuietNightCommand(game.CurrentPlayer.Role)));
        }

        [Test]
        public void Dispatcher_moves_self_to_other_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var medic = game.PlayerByRole(Role.Medic);
            game = game with { Players = game.Players.Replace(medic, medic with { Location = "Paris" }) };
            var events = new List<IEvent>();

            game = game.Do(new DispatcherMovePawnToOtherPawnCommand(Role.Dispatcher, Role.Medic), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.CurrentPlayer.Location.ShouldBe("Paris");
        }

        [Test]
        public void Dispatcher_moves_other_pawn_to_self()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var medic = game.PlayerByRole(Role.Medic);
            game = game with { Players = game.Players.Replace(medic, medic with { Location = "Paris" }) };
            var events = new List<IEvent>();

            game = game.Do(new DispatcherMovePawnToOtherPawnCommand(Role.Medic, Role.Dispatcher), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Medic).Location.ShouldBe("Atlanta");
        }

        [Test]
        public void Dispatcher_move_pawn_to_pawn_throws_when_already_at_destination()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherMovePawnToOtherPawnCommand(Role.Dispatcher, Role.Medic), events));
        }

        [Test]
        public void Dispatcher_move_pawn_to_pawn_throws_if_not_dispatchers_turn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Medic, Role.Dispatcher } });
            var medic = game.PlayerByRole(Role.Medic);
            game = game with { Players = game.Players.Replace(medic, medic with { Location = "Paris" }) };
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherMovePawnToOtherPawnCommand(Role.Dispatcher, Role.Medic), events));
        }

        [Test]
        public void Dispatcher_moves_other_pawn_to_other_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Dispatcher, Role.Medic, Role.Scientist }
            });
            var medic = game.PlayerByRole(Role.Medic);
            var scientist = game.PlayerByRole(Role.Scientist);
            game = game with
            {
                Players = game.Players
                    .Replace(medic, medic with { Location = "Paris" })
                    .Replace(scientist, scientist with { Location = "Bogota" })
            };
            var events = new List<IEvent>();

            game = game.Do(new DispatcherMovePawnToOtherPawnCommand(Role.Medic, Role.Scientist), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Medic).Location.ShouldBe("Bogota");
        }

        [Test]
        public void Dispatcher_drive_ferry_other_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var events = new List<IEvent>();

            game = game.Do(new DispatcherDriveFerryPawnCommand(Role.Medic, "Chicago"), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Medic).Location.ShouldBe("Chicago");
        }

        [Test]
        public void Dispatcher_drive_ferry_other_pawn_throws_if_used_on_dispatcher()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherDriveFerryPawnCommand(Role.Dispatcher, "Chicago"), events));
        }

        [Test]
        public void Dispatcher_drive_ferry_other_pawn_throws_if_not_dispatchers_turn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Medic, Role.Dispatcher } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherDriveFerryPawnCommand(Role.Medic, "Chicago"), events));
        }

        [Test]
        public void Dispatcher_drive_ferry_other_pawn_throws_not_adjacent()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherDriveFerryPawnCommand(Role.Medic, "Moscow"), events));
        }

        [Test]
        public void Dispatcher_direct_fly_other_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(chicago) });
            var events = new List<IEvent>();

            game = game.Do(new DispatcherDirectFlyPawnCommand(Role.Medic, "Chicago"), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Medic).Location.ShouldBe("Chicago");
            game.CurrentPlayer.Hand.ShouldNotContain(chicago);
            game.PlayerDiscardPile.TopCard.ShouldBe(chicago);
        }

        [Test]
        public void Dispatcher_direct_fly_throws_if_used_on_self()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of(PlayerCards.CityCard("Chicago"))
            });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherDirectFlyPawnCommand(Role.Dispatcher, "Chicago"), events));
        }

        [Test]
        public void Dispatcher_direct_fly_throws_if_player_is_at_destination()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of(PlayerCards.CityCard("Chicago"))
            }).SetPlayer(Role.Medic, game.PlayerByRole(Role.Medic) with { Location = "Chicago" });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherDirectFlyPawnCommand(Role.Medic, "Chicago"), events));
        }

        [Test]
        public void Dispatcher_direct_fly_throws_if_card_not_in_hand()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Empty });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherDirectFlyPawnCommand(Role.Medic, "Chicago"), events));
        }

        [Test]
        public void Dispatcher_charter_fly_other_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var atlanta = PlayerCards.CityCard("Atlanta");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(atlanta) });
            var events = new List<IEvent>();

            game = game.Do(new DispatcherCharterFlyPawnCommand(Role.Medic, "Paris"), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Medic).Location.ShouldBe("Paris");
            game.CurrentPlayer.Hand.ShouldNotContain(atlanta);
            game.PlayerDiscardPile.TopCard.ShouldBe(atlanta);
        }

        [Test]
        public void Dispatcher_charter_fly_other_throws_when_used_on_self()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var atlanta = PlayerCards.CityCard("Atlanta");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(atlanta) });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherCharterFlyPawnCommand(Role.Dispatcher, "Paris"), events));
        }

        [Test]
        public void Dispatcher_charter_fly_other_throws_when_player_is_not_on_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var atlanta = PlayerCards.CityCard("Atlanta");
            game = game
                .SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(atlanta) })
                .SetPlayer(Role.Medic, game.PlayerByRole(Role.Medic) with { Location = "Chicago" });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherCharterFlyPawnCommand(Role.Medic, "Paris"), events));
        }

        [Test]
        public void Dispatcher_charter_fly_other_pawn_throws_if_already_at_destination()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var atlanta = PlayerCards.CityCard("Atlanta");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(atlanta) });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherCharterFlyPawnCommand(Role.Medic, "Atlanta"), events));
        }

        [Test]
        public void Dispatcher_shuttle_fly_other_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var moscow = game.CityByName("Moscow");
            game = game with { Cities = game.Cities.Replace(moscow, moscow with { HasResearchStation = true }) };
            var events = new List<IEvent>();

            game = game.Do(new DispatcherShuttleFlyPawnCommand(Role.Medic, "Moscow"), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.PlayerByRole(Role.Medic).Location.ShouldBe("Moscow");
        }

        [Test]
        public void Dispatcher_shuttle_fly_throws_when_used_on_self()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var moscow = game.CityByName("Moscow");
            game = game with { Cities = game.Cities.Replace(moscow, moscow with { HasResearchStation = true }) };
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherShuttleFlyPawnCommand(Role.Dispatcher, "Moscow"), events));
        }

        [Test]
        public void Dispatcher_shuttle_fly_other_pawn_throws_if_not_on_research_station()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var moscow = game.CityByName("Moscow");
            game = game with { Cities = game.Cities.Replace(moscow, moscow with { HasResearchStation = true }) };
            game = game.SetPlayer(Role.Medic, game.PlayerByRole(Role.Medic) with { Location = "Chicago" });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherShuttleFlyPawnCommand(Role.Medic, "Moscow"), events));
        }

        [Test]
        public void Dispatcher_shuttle_fly_other_pawn_throws_when_destination_has_no_station()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherShuttleFlyPawnCommand(Role.Medic, "Moscow"), events));
        }

        [Test]
        public void Dispatcher_shuttle_fly_other_pawn_throws_if_already_at_destination()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new DispatcherShuttleFlyPawnCommand(Role.Medic, "Atlanta"), events));
        }

        [Test]
        public void Operations_expert_builds_research_station_without_card()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" });
            var events = new List<IEvent>();

            game = game.Do(new OperationsExpertBuildResearchStation(), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.CityByName("Chicago").HasResearchStation.ShouldBeTrue();
            game.ResearchStationPile.ShouldBe(4);
        }

        [Test]
        public void Operations_expert_builds_research_station_throws_when_already_station()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() => game.Do(new OperationsExpertBuildResearchStation(), events));
        }

        [Test]
        public void Operations_expert_builds_research_station_throws_when_no_stations_left()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" });
            game = game with { ResearchStationPile = 0 };
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() => game.Do(new OperationsExpertBuildResearchStation(), events));
        }

        [Test]
        public void Operations_expert_moves_from_station_to_anywhere()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(chicago) });
            var events = new List<IEvent>();

            game = game.Do(new OperationsExpertDiscardToMoveFromStation(chicago, "Paris"), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.CurrentPlayer.Hand.ShouldNotContain(chicago);
            game.PlayerDiscardPile.TopCard.ShouldBe(chicago);
        }

        [Test]
        public void Operations_expert_moves_from_station_throws_when_not_on_station()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of(chicago),
                Location = "Paris"
            });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new OperationsExpertDiscardToMoveFromStation(chicago, "Moscow"), events));
        }

        [Test]
        public void Operations_expert_moves_from_station_to_anywhere_only_once_per_turn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            var chicagoCard = PlayerCards.CityCard("Chicago");
            var sydneyCard = PlayerCards.CityCard("Sydney");
            var moscowCity = game.CityByName("Moscow");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(chicagoCard, sydneyCard) });
            game = game with
            {
                Cities = game.Cities.Replace(moscowCity, moscowCity with { HasResearchStation = true }),
                PlayerDrawPile = game.PlayerDrawPile.RemoveIfPresent(chicagoCard).RemoveIfPresent(sydneyCard)
            };
            var events = new List<IEvent>();

            // act: use discard + move ability
            game = game.Do(new OperationsExpertDiscardToMoveFromStation(chicagoCard, "Moscow"), events);

            // assert: can't use again this turn
            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new OperationsExpertDiscardToMoveFromStation(sydneyCard, "Paris"), events));

            // act: pass turn
            game = game.Do(new PassCommand(Role.OperationsExpert), events);
            // act: pass turn (medic)
            game = game.Do(new PassCommand(Role.Medic), events);
            game.CurrentPlayer.Role.ShouldBe(Role.OperationsExpert);

            // act: use discard + move ability that failed last time
            game = game.Do(new OperationsExpertDiscardToMoveFromStation(sydneyCard, "Paris"), events);

            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            game.CurrentPlayer.Hand.ShouldNotContain(sydneyCard);
            game.PlayerDiscardPile.TopCard.ShouldBe(sydneyCard);
            game.CurrentPlayer.Location.ShouldBe("Paris");
        }

        [Test]
        public void Operations_expert_move_from_station_throws_if_at_destination()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(chicago) });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new OperationsExpertDiscardToMoveFromStation(chicago, "Atlanta"), events));
        }

        [Test]
        public void Operations_expert_move_from_station_to_anywhere_throws_if_card_not_in_hand()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.OperationsExpert, Role.Medic } });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Empty });
            var events = new List<IEvent>();

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new OperationsExpertDiscardToMoveFromStation(chicago, "Paris"), events));
        }

        [Test]
        public void Medic_treat_disease_removes_all_cubes()
        {
            var game = DefaultTestGame();
            var atlanta = game.CityByName("Atlanta");
            game = game with
            {
                Cities = game.Cities.Replace(atlanta, atlanta with { Cubes = CubePile.Empty.AddCubes(Colour.Blue, 3) })
            };
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);

            (game, _) = game.Do(new TreatDiseaseCommand(game.CurrentPlayer.Role, "Atlanta", Colour.Blue));

            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 3);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__drive_ferry()
        {
            var game = DefaultTestGame();
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue);
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, _) = game.Do(new DriveFerryCommand(game.CurrentPlayer.Role, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes__only_for_cured_diseases()
        {
            var game = DefaultTestGame();
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .AddCube("Chicago", Colour.Red);
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, _) = game.Do(new DriveFerryCommand(game.CurrentPlayer.Role, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.CityByName("Chicago").Cubes.NumberOf(Colour.Red).ShouldBe(1);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
        }

        [Test]
        public void Medic_auto_removes_cubes_for_multiple_cured_diseases()
        {
            var game = DefaultTestGame();
            game = game
                .Cure(Colour.Blue)
                .Cure(Colour.Red)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .AddCube("Chicago", Colour.Red);
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);
            var startingRedCubes = game.Cubes.NumberOf(Colour.Red);

            (game, _) = game.Do(new DriveFerryCommand(game.CurrentPlayer.Role, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.CityByName("Chicago").Cubes.NumberOf(Colour.Red).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.Cubes.NumberOf(Colour.Red).ShouldBe(startingRedCubes + 1);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__shuttle()
        {
            var game = DefaultTestGame();
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue);
            game = game with
            {
                Cities = game.Cities.Replace(game.CityByName("Chicago"),
                    game.CityByName("Chicago") with { HasResearchStation = true })
            };
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new ShuttleFlightCommand(game.CurrentPlayer.Role, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__airlift()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Researcher, Role.Medic } });
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .SetCurrentPlayerAs(game.CurrentPlayer with { Hand = game.CurrentPlayer.Hand.Add(new AirliftCard()) });
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new AirliftCommand(Role.Researcher, Role.Medic, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__direct_flight()
        {
            var game = DefaultTestGame();
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Chicago") });
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new DirectFlightCommand(game.CurrentPlayer.Role, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__charter_flight()
        {
            var game = DefaultTestGame();
            var atlanta = PlayerCards.CityCard("Atlanta");
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(atlanta) });
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new CharterFlightCommand(game.CurrentPlayer.Role, atlanta, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__dispatcher_drive_ferry()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue);
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, _) = game.Do(new DispatcherDriveFerryPawnCommand(Role.Medic, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__dispatcher_charter_flight()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            var atlanta = PlayerCards.CityCard("Atlanta");
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of(atlanta) });
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new DispatcherCharterFlyPawnCommand(Role.Medic, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__dispatcher_direct_flight()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue)
                .SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Chicago") });
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new DispatcherDirectFlyPawnCommand(Role.Medic, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__dispatcher_move_pawn()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Dispatcher, Role.Medic }
            });
            var medic = game.PlayerByRole(Role.Medic);
            var dispatcher = game.PlayerByRole(Role.Dispatcher);
            game = game
                    .Cure(Colour.Yellow)
                    .RemoveAllCubesFromCities()
                    .AddCube("Bogota", Colour.Yellow) with
                {
                    Players = game.Players
                        .Replace(medic, medic with { Location = "Paris" })
                        .Replace(dispatcher, dispatcher with { Location = "Bogota" })
                };
            var startingYellowCubes = game.Cubes.NumberOf(Colour.Yellow);
            var events = new List<IEvent>();

            game = game.Do(new DispatcherMovePawnToOtherPawnCommand(Role.Medic, Role.Dispatcher), events);

            game.CityByName("Bogota").Cubes.NumberOf(Colour.Yellow).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Yellow).ShouldBe(startingYellowCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cured__dispatcher_shuttle()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue);
            game = game with
            {
                Cities = game.Cities.Replace(game.CityByName("Chicago"),
                    game.CityByName("Chicago") with { HasResearchStation = true })
            };
            var startingBlueCubes = game.Cubes.NumberOf(Colour.Blue);

            (game, var events) = game.Do(new DispatcherShuttleFlyPawnCommand(Role.Medic, "Chicago"));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
            game.Cubes.NumberOf(Colour.Blue).ShouldBe(startingBlueCubes + 1);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Medic_auto_removes_cubes_when_cure_is_discovered()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with { Roles = new[] { Role.Dispatcher, Role.Medic } });
            game = game
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue);
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta", "Chicago", "Montreal", "Paris", "Milan")
            });
            game = game.SetPlayer(Role.Medic, game.PlayerByRole(Role.Medic) with { Location = "Chicago" });

            (game, var events) =
                game.Do(new DiscoverCureCommand(Role.Dispatcher, game.CurrentPlayer.Hand.CityCards.ToArray()));

            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Medic_prevents_infect_when_cured()
        {
            var game = DefaultTestGame()
                .RemoveAllCubesFromCities()
                .Cure(Colour.Blue);
            game = game with
            {
                InfectionDrawPile = game.InfectionDrawPile.PlaceOnTop(new InfectionCard("Atlanta", Colour.Blue))
            };
            var events = new List<IEvent>();

            game = game.Do(new PassCommand(Role.Medic), events);

            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Medic_prevents_outbreak_adjacent_infect_when_cured()
        {
            var game = DefaultTestGame().Cure(Colour.Blue);

            var atlantaInfectionCard = InfectionCard.FromCity(game.Board.City("Atlanta"));
            game = game
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3) with
                {
                    InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlantaInfectionCard)
                        .PlaceOnTop(atlantaInfectionCard),
                };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" });

            // act
            (game, _) = game.Do(new PassCommand(Role.Medic));

            game.OutbreakCounter.ShouldBe(1);
            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Medic_prevents_epidemic_infect_when_cured()
        {
            var game = DefaultTestGame()
                .RemoveAllCubesFromCities()
                .Cure(Colour.Blue);

            game = game with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(new EpidemicCard()),
                InfectionDrawPile = game.InfectionDrawPile.PlaceAtBottom(new InfectionCard("Atlanta", Colour.Blue))
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            (game, var events) = game.Do(new PassCommand(Role.Medic));

            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Medic_auto_removes_cubes_eradicates__drive_ferry()
        {
            var game = DefaultTestGame();
            game = game
                .Cure(Colour.Blue)
                .RemoveAllCubesFromCities()
                .AddCube("Chicago", Colour.Blue);

            (game, _) = game.Do(new DriveFerryCommand(game.CurrentPlayer.Role, "Chicago"));

            game.IsEradicated(Colour.Blue).ShouldBeTrue();
        }

        [Test]
        public void Researcher_can_share_any_card()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Researcher, Role.Scientist }
            });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of(chicago)
            });

            (game, _) = game.Do(new ResearcherShareKnowledgeGiveCommand(Role.Scientist, "Chicago"));

            game.PlayerByRole(Role.Scientist).Hand.CityCards.ShouldContain(chicago);
            game.PlayerByRole(Role.Researcher).Hand.CityCards.ShouldNotContain(chicago);
            game.PlayerByRole(Role.Researcher).ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Researcher_share_throws_if_not_in_hand()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Researcher, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Empty
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ResearcherShareKnowledgeGiveCommand(Role.Scientist, "Chicago")));
        }

        [Test]
        public void Researcher_share_knowledge_receiver_must_discard_if_more_than_7_cards()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Researcher, Role.Scientist }
            });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of(chicago)
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of(PlayerCards.CityCards.Shuffle().Take(7))
            });

            // act
            (game, _) = game.Do(new ResearcherShareKnowledgeGiveCommand(Role.Scientist, "Chicago"));

            var generator = new PlayerCommandGenerator();
            generator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Scientist);
        }

        [Test]
        public void Researcher_share_knowledge_throws_when_share_to_self()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Researcher, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Chicago")
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ResearcherShareKnowledgeGiveCommand(Role.Researcher, "Chicago")));
        }

        [Test]
        public void Researcher_share_knowledge_throws_when_other_player_not_in_same_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Researcher, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Chicago"),
                Location = "Chicago"
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ResearcherShareKnowledgeGiveCommand(Role.Scientist, "Chicago")));
        }

        [Test]
        public void Players_can_take_any_card_from_researcher()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Researcher }
            });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetPlayer(Role.Researcher, game.PlayerByRole(Role.Researcher) with
            {
                Hand = PlayerHand.Of(chicago)
            });

            (game, _) = game.Do(new ShareKnowledgeTakeFromResearcherCommand(Role.Scientist, "Chicago"));

            game.PlayerByRole(Role.Scientist).Hand.CityCards.ShouldContain(chicago);
            game.PlayerByRole(Role.Researcher).Hand.CityCards.ShouldNotContain(chicago);
            game.PlayerByRole(Role.Scientist).ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Researcher_share_knowledge_take_throws_if_researcher_does_not_have_card()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Researcher }
            });
            game = game.SetPlayer(Role.Researcher, game.PlayerByRole(Role.Researcher) with
            {
                Hand = PlayerHand.Empty
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ShareKnowledgeTakeFromResearcherCommand(Role.Scientist, "Chicago")));
        }

        [Test]
        public void Researcher_share_knowledge_take_throws_if_not_in_same_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Researcher }
            });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetPlayer(Role.Researcher, game.PlayerByRole(Role.Researcher) with
            {
                Hand = PlayerHand.Of(chicago),
                Location = "Chicago"
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ShareKnowledgeTakeFromResearcherCommand(Role.Scientist, "Chicago")));
        }

        [Test]
        public void Researcher_share_knowledge_taker_must_discard_if_more_than_7_cards()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Researcher }
            });
            var chicago = PlayerCards.CityCard("Chicago");
            game = game.SetPlayer(Role.Researcher, game.PlayerByRole(Role.Researcher) with
            {
                Hand = PlayerHand.Of(chicago)
            }).SetPlayer(Role.Scientist, game.PlayerByRole(Role.Scientist) with
            {
                Hand = PlayerHand.Of(PlayerCards.CityCards.Shuffle().Take(7))
            });

            // act
            (game, _) = game.Do(new ShareKnowledgeTakeFromResearcherCommand(Role.Scientist, "Chicago"));

            var generator = new PlayerCommandGenerator();
            generator.LegalCommands(game).ShouldAllBe(c => c is DiscardPlayerCardCommand && c.Role == Role.Scientist);
        }

        [Test]
        public void Quarantine_specialist_prevents_infection_in_current_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });
            var atlanta = new InfectionCard("Atlanta", Colour.Blue);
            game = game.RemoveAllCubesFromCities() with
            {
                InfectionDrawPile = game.InfectionDrawPile.RemoveIfPresent(atlanta).PlaceOnTop(atlanta)
            };
            var events = new List<IEvent>();

            // act
            game = game.Do(new PassCommand(Role.QuarantineSpecialist), events);

            // assert
            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Quarantine_specialist_prevents_infection_in_adjacent_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });
            var atlanta = new InfectionCard("Atlanta", Colour.Blue);
            game = game
                    .RemoveAllCubesFromCities()
                    .SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" }) with
                {
                    InfectionDrawPile = game.InfectionDrawPile.RemoveIfPresent(atlanta).PlaceOnTop(atlanta)
                };
            var events = new List<IEvent>();

            // act
            game = game.Do(new PassCommand(Role.QuarantineSpecialist), events);

            // assert
            game.CityByName("Atlanta").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Quarantine_specialist_prevents_outbreak_infection_when_in_adjacent_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });

            var atlantaInfectionCard = InfectionCard.FromCity(game.Board.City("Atlanta"));
            game = game
                    .SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Montreal" })
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3) with
                {
                    InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlantaInfectionCard)
                        .PlaceOnTop(atlantaInfectionCard),
                };

            // act
            (game, var events) = game.Do(new PassCommand(Role.QuarantineSpecialist));

            game.OutbreakCounter.ShouldBe(1);
            game.CityByName("Chicago").Cubes.NumberOf(Colour.Blue).ShouldBe(0);
        }

        [Test]
        public void Quarantine_specialist_prevents_outbreaks_when_in_current_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });

            var atlantaInfectionCard = InfectionCard.FromCity(game.Board.City("Atlanta"));
            game = game
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3) with
                {
                    InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlantaInfectionCard)
                        .PlaceOnTop(atlantaInfectionCard),
                };

            // act
            (game, var events) = game.Do(new PassCommand(Role.QuarantineSpecialist));

            game.OutbreakCounter.ShouldBe(0);
            game.Board.AdjacentCities["Atlanta"].Select(c => game.CityByName(c))
                .ShouldAllBe(c => c.Cubes.NumberOf(Colour.Blue) == 0);
        }

        [Test]
        public void Quarantine_specialist_prevents_outbreaks_when_in_adjacent_city()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });

            var atlantaInfectionCard = InfectionCard.FromCity(game.Board.City("Atlanta"));
            game = game
                    .SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Chicago" })
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3) with
                {
                    InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlantaInfectionCard)
                        .PlaceOnTop(atlantaInfectionCard),
                };

            // act
            (game, var events) = game.Do(new PassCommand(Role.QuarantineSpecialist));

            game.OutbreakCounter.ShouldBe(0);
            game.Board.AdjacentCities["Atlanta"].Select(c => game.CityByName(c))
                .ShouldAllBe(c => c.Cubes.NumberOf(Colour.Blue) == 0);
        }

        [TestCase("Atlanta")]
        [TestCase("Chicago")]
        public void Quarantine_specialist_prevents_outbreak_when_epidemic_causes_outbreak(string quarantineSpecialistLocation)
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });
            game = game.RemoveAllCubesFromCities() with
            {
                PlayerDrawPile = game.PlayerDrawPile.PlaceOnTop(
                    PlayerCards.CityCard("Atlanta"),
                    new EpidemicCard()),
                InfectionDiscardPile = Deck<InfectionCard>.Empty,
                InfectionDrawPile = game.InfectionDrawPile.PlaceAtBottom(new InfectionCard("Atlanta", Colour.Blue)),
                InfectionRateMarkerPosition = 5, // ensure that epidemic city is infected immediately
            };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = quarantineSpecialistLocation });

            // act
            (game, var events) = game.Do(new PassCommand(Role.QuarantineSpecialist));

            // assert
            game.OutbreakCounter.ShouldBe(0);
        }

        [Test]
        public void Quarantine_specialist_prevents_outbreak_during_chain_reaction()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.QuarantineSpecialist, Role.Scientist }
            });

            var atlanta = new InfectionCard("Atlanta", Colour.Blue);
            var chicago = new InfectionCard("Chicago", Colour.Blue);
            game = game
                    .RemoveAllCubesFromCities()
                    .AddCubes("Atlanta", Colour.Blue, 3)
                    .AddCubes("Chicago", Colour.Blue, 3) with
                {
                    InfectionDrawPile =
                    game.InfectionDrawPile
                        .RemoveIfPresent(atlanta).PlaceOnTop(atlanta)
                        .RemoveIfPresent(chicago),
                };
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Location = "Montreal" });

            (game, var events) = game.Do(new PassCommand(Role.QuarantineSpecialist));

            game.OutbreakCounter.ShouldBe(1); // one outbreak at atlanta, no outbreak at chicago
        }

        [Test]
        public void Scientist_cures_with_4_cards()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Dispatcher }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(4))
            });

            (game, _) = game.Do(
                new ScientistDiscoverCureCommand(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));

            Assert.IsTrue(game.IsCured(Colour.Black));
            Assert.AreEqual(0, game.CurrentPlayer.Hand.Count);
            Assert.AreEqual(4, game.PlayerDiscardPile.Count);
            Assert.AreEqual(3, game.CurrentPlayer.ActionsRemaining);
        }

        [Test]
        public void Scientist_cure_throws_if_already_cured()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Dispatcher }
            }).Cure(Colour.Black);

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(4))
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ScientistDiscoverCureCommand(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Scientist_cure_last_disease_wins()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
                {
                    Roles = new[] { Role.Scientist, Role.Dispatcher }
                })
                .Cure(Colour.Blue)
                .Cure(Colour.Red)
                .Cure(Colour.Yellow);

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(4)),
            });

            (game, _) = game.Do(
                new ScientistDiscoverCureCommand(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));

            Assert.IsTrue(game.IsWon);
        }

        [Test]
        public void Scientist_cure_throws_if_not_at_station()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Dispatcher }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Chicago",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(4))
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ScientistDiscoverCureCommand(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Scientist_cure_throws_if_not_enough_cards()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Dispatcher }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(3))
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ScientistDiscoverCureCommand(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Scientist_cure_throws_if_cards_are_not_same_colour()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.Scientist, Role.Dispatcher }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Hand = PlayerHand.Of("Atlanta", "Chicago", "Essen", "Miami")
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ScientistDiscoverCureCommand(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray())));
        }

        [Test]
        public void Contingency_planner_can_take_event_card_from_discard_pile_and_use_it()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.ContingencyPlanner, Role.Dispatcher }
            });
            var airlift = new AirliftCard();
            game = game with { PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(airlift) };

            // act: take card
            (game, var events) = game.Do(new ContingencyPlannerTakeEventCardCommand(airlift));

            // assert
            ((ContingencyPlanner)game.CurrentPlayer).StoredEventCard.ShouldBe(airlift);
            game.CurrentPlayer.Hand.ShouldNotContain(airlift); // this event card is not stored in hand
            game.PlayerDiscardPile.Cards.ShouldNotContain(airlift);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);

            // act: use card
            (game, events) = game.Do(new ContingencyPlannerSpecialEventCommand(
                new AirliftCommand(Role.ContingencyPlanner, Role.Dispatcher, "Chicago")));

            // assert
            game.PlayerByRole(Role.Dispatcher).Location.ShouldBe("Chicago");
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
            ((ContingencyPlanner)game.CurrentPlayer).StoredEventCard.ShouldBeNull();
            game.PlayerDiscardPile.Cards.ShouldNotContain(airlift); // card is removed from game
            game.PlayerDrawPile.Cards.ShouldNotContain(airlift);
        }

        [Test]
        public void Contingency_planner_take_event_card_throws_if_not_in_pile()
        {
            var game = DefaultTestGame(DefaultTestGameOptions() with
            {
                Roles = new[] { Role.ContingencyPlanner, Role.Dispatcher }
            });

            Should.Throw<GameRuleViolatedException>(() =>
                game.Do(new ContingencyPlannerTakeEventCardCommand(new AirliftCard())));
        }

        private static int TotalNumCubesOnCities(PandemicGame game)
        {
            return game.Cities.Sum(c => c.Cubes.Counts.Sum(cc => cc.Value));
        }

        private static void AssertEndsTurn(Func<(PandemicGame, IEnumerable<IEvent>)> action)
        {
            IEnumerable<IEvent> events;

            (_, events) = action();

            Assert.IsTrue(events.Any(e => e is TurnEnded));
        }

        private static NewGameOptions DefaultTestGameOptions()
        {
            return new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist },
                IncludeSpecialEventCards = false
            };
        }

        private static PandemicGame DefaultTestGame()
        {
            return DefaultTestGame(DefaultTestGameOptions());
        }

        private static PandemicGame DefaultTestGame(NewGameOptions options)
        {
            var (game, _) = PandemicGame.CreateNewGame(options);

            game = game.WithNoEpidemics();

            // allowing invalid game states makes many test scenarios much easier
            // to set up
            return game with { SelfConsistencyCheckingEnabled = false };
        }
    }
}
