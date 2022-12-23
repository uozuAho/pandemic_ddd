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

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, toCity);

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
                game.DriveOrFerryPlayer(Role.Medic, "fasdfasdf"));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(Role.Medic, "Beijing"));
        }

        [Test]
        public void Drive_or_ferry_can_end_turn()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { ActionsRemaining = 1 });

            AssertEndsTurn(() => game.DriveOrFerryPlayer(Role.Medic, "Chicago"));
        }

        [Test]
        public void Direct_flight_goes_to_city_and_discards_card()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Miami") });

            (game, _) = game.DirectFlight(game.CurrentPlayer.Role, "Miami");

            Assert.That(game.CurrentPlayer.Location, Is.EqualTo("Miami"));
            Assert.That(game.CurrentPlayer.Hand, Has.No.Member(PlayerCards.CityCard("Miami")));
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
                () => game.DirectFlight(game.CurrentPlayer.Role, "Miami"),
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
                () => game.DirectFlight(game.CurrentPlayer.Role, "Atlanta"),
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

            AssertEndsTurn(() => game.DirectFlight(Role.Medic, "Miami"));
        }

        [Test]
        public void Charter_flight_goes_to_city_and_discards_card()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

            (game, _) = game.CharterFlight(game.CurrentPlayer.Role, "Bogota");

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
                game.CharterFlight(Role.Medic, "fasdfasdf"));
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
                game.CharterFlight(Role.Medic, "Bogota"));
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
                game.CharterFlight(Role.Scientist, "Bogota"));
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

            AssertEndsTurn(() => game.CharterFlight(Role.Medic, "Bogota"));
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
        {
            var startingState = NewGameWithNoEpidemicCards();

            var game = startingState.SetCurrentPlayerAs(
                startingState.CurrentPlayer with { ActionsRemaining = 1 });

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");

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

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DriveOrFerryPlayer(Role.Medic, "Chicago"));
        }

        [Test]
        public void Cities_are_infected_after_player_turn_ends()
        {
            var (startingState, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var (game, _) = startingState.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.AreEqual(startingState.InfectionDrawPile.Count - 2, game.InfectionDrawPile.Count);
            Assert.AreEqual(startingState.InfectionDiscardPile.Count + 2, game.InfectionDiscardPile.Count);

            foreach (var infectionCard in game.InfectionDiscardPile.Top(2))
            {
                var city = game.CityByName(infectionCard.City.Name);
                Assert.That(city.Cubes[infectionCard.City.Colour], Is.EqualTo(1),
                    $"{infectionCard.City.Name} should have had 1 {infectionCard.City.Colour} cube added");
            }

            Assert.That(game.Cubes.Values.Sum(), Is.EqualTo(startingState.Cubes.Values.Sum() - 2));
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
                Cubes = ColourExtensions.AllColours.ToImmutableDictionary(c => c, _ => 0)
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

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

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            game.PlayerByRole(Role.Medic).ActionsRemaining.ShouldBe(4,
                "player whose turn ended should get their 'remaining actions' counter reset");
            game.CurrentPlayer.Role.ShouldBe(Role.Scientist);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(4);
        }

        [Test]
        public void Player_must_discard_when_hand_is_full()
        {
            var game = NewGameWithNoEpidemicCards();

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(7))
            });

            // act
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");

            // assert
            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(0);
            new PlayerCommandGenerator().LegalCommands(game).ShouldAllBe(move => move is DiscardPlayerCardCommand);
        }

        [Test]
        public void Discard_player_card_goes_to_discard_pile()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                ActionsRemaining = 1,
                Hand = new PlayerHand(game.PlayerDrawPile.Top(7))
            });
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");

            // act
            var cardToDiscard = game.CurrentPlayer.Hand.First();
            (game, _) = game.DiscardPlayerCard(cardToDiscard);

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
                Hand = new PlayerHand(initialGame.PlayerDrawPile.Top(6))
            });

            (game, _) = game.DiscardPlayerCard(game.CurrentPlayer.Hand.First());

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

            (game, _) = game.DiscardPlayerCard(game.CurrentPlayer.Hand.First());

            game.CurrentPlayer.Role.ShouldBe(Role.Medic);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(0);
            new PlayerCommandGenerator().LegalCommands(game).ShouldAllBe(move => move is DiscardPlayerCardCommand);
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
            (game, _) = game.BuildResearchStation("Chicago");

            game.CityByName("Chicago").HasResearchStation.ShouldBe(true);
            game.CurrentPlayer.Hand.ShouldNotContain(chicagoPlayerCard);
            game.PlayerDiscardPile.Cards.ShouldContain(chicagoPlayerCard);
            game.CurrentPlayer.ActionsRemaining.ShouldBe(3);
        }

        [Test]
        public void Build_research_station_can_end_turn()
        {
            var game = NewGameWithNoEpidemicCards();

            var chicagoPlayerCard = PlayerCards.CityCard("Chicago");

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Chicago",
                Hand = PlayerHand.Of(chicagoPlayerCard),
                ActionsRemaining = 1
            });

            AssertEndsTurn(() => game.BuildResearchStation("Chicago"));
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

            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Chicago"));
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
            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Atlanta"));
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
            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Atlanta"));
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

            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Chicago"));
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

            (game, _) = game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray());

            Assert.IsTrue(game.CureDiscovered[Colour.Black]);
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

            AssertEndsTurn(() => game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
        }

        [Test]
        public void Cure_last_disease_wins()
        {
            var game = NewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist }
                }) with
                {
                    CureDiscovered = new Dictionary<Colour, bool>
                    {
                        { Colour.Black, false },
                        { Colour.Blue, true },
                        { Colour.Red, true },
                        { Colour.Yellow, true }
                    }.ToImmutableDictionary()
                };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5)),
            });

            // act
            (game, _) = game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray());

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
                    CureDiscovered = new Dictionary<Colour, bool>
                    {
                        { Colour.Black, false },
                        { Colour.Blue, true },
                        { Colour.Red, true },
                        { Colour.Yellow, true }
                    }.ToImmutableDictionary()
                };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5)),
                ActionsRemaining = 1
            });

            // act
            (game, _) = game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray());

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
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
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
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
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
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
        }

        [Test]
        public void Cure_already_cured_disease_throws()
        {
            var game = NewGame(new NewGameOptions
                {
                    Roles = new[] { Role.Medic, Role.Scientist }
                }) with
                {
                    CureDiscovered = new Dictionary<Colour, bool>
                    {
                        { Colour.Black, true },
                        { Colour.Blue, false },
                        { Colour.Red, false },
                        { Colour.Yellow, false }
                    }.ToImmutableDictionary()
                };

            game = game.SetCurrentPlayerAs(game.CurrentPlayer with
            {
                Location = "Atlanta",
                Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
            });

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
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

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");

            Assert.IsFalse(game.PlayerByRole(Role.Medic).Hand.Any(c => c is EpidemicCard));
            Assert.AreEqual(1, game.PlayerDiscardPile.Cards.Count(c => c is EpidemicCard));
        }

        private static int TotalNumCubesOnCities(PandemicGame game)
        {
            return game.Cities.Sum(c => c.Cubes.Sum(cc => cc.Value));
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

        private static PandemicGame NewGameWithNoEpidemicCards()
        {
            var game = NewGame(new NewGameOptions
            {
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            return game with
            {
                PlayerDrawPile = new Deck<PlayerCard>(PlayerCards.CityCards)
            };
        }
    }
}
