using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.test
{
    internal class GameTests
    {
        [TestCase("Chicago")]
        [TestCase("Washington")]
        public void Drive_or_ferry_player_moves_them_to_city(string toCity)
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, toCity);

            Assert.AreEqual(toCity, game.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Drive_or_ferry_to_garbage_city_throws()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(Role.Medic, "fasdfasdf"));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(Role.Medic, "Beijing"));
        }

        [Test]
        public void Drive_or_ferry_can_end_turn()
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
                    ActionsRemaining = 1
                })
            };

            AssertEndsTurn(() => game.DriveOrFerryPlayer(Role.Medic, "Chicago"));
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
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

            Assert.AreEqual(
                startingState.PlayerByRole(Role.Medic).Hand.Count + 2,
                game.PlayerByRole(Role.Medic).Hand.Count);
        }

        [Test]
        public void Player_attempts_fifth_action_throws()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] {Role.Medic, Role.Scientist}
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
            Assert.AreEqual(2, game.InfectionDiscardPile.Count);

            foreach (var infectionCard in game.InfectionDiscardPile.TakeLast(2))
            {
                Assert.AreEqual(1, game.CityByName(infectionCard.City.Name).Cubes[infectionCard.City.Colour],
                    $"{infectionCard.City.Name} should have had 1 {infectionCard.City.Colour} cube added");
            }

            Assert.AreEqual(24 * 4 - 2, game.Cubes.Values.Sum(), "2 cubes should have been removed from cube pile");
        }

        [Test]
        public void Game_ends_when_cubes_run_out()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            game = game with
            {
                Cubes = Enum.GetValues<Colour>().ToImmutableDictionary(c => c, _ => 0)
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.IsTrue(game.IsOver);
            Assert.AreEqual(1, game.InfectionDiscardPile.Count);
        }

        [Test]
        public void It_is_next_players_turn_after_infect_cities()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] {Role.Medic, Role.Scientist}
            });

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.AreEqual(4, game.PlayerByRole(Role.Medic).ActionsRemaining,
                "player whose turn ended should get their 'remaining actions' counter reset");
            Assert.AreEqual(Role.Scientist, game.CurrentPlayer.Role);
            Assert.AreEqual(4, game.CurrentPlayer.ActionsRemaining);
        }

        [Test]
        public void Player_must_discard_when_hand_is_full()
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
                    Hand = new PlayerHand(game.PlayerDrawPile.Take(7))
                })
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.AreEqual(Role.Medic, game.CurrentPlayer.Role);
            Assert.AreEqual(0, game.CurrentPlayer.ActionsRemaining);
            Assert.True(new PlayerCommandGenerator().LegalCommands(game).All(move => move is DiscardPlayerCardCommand));
        }

        [Test]
        public void Discard_player_card_goes_to_discard_pile()
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
                    Hand = new PlayerHand(game.PlayerDrawPile.Take(7))
                })
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            // act
            var cardToDiscard = game.CurrentPlayer.Hand.First();
            (game, _) = game.DiscardPlayerCard(cardToDiscard);

            Assert.IsFalse(game.CurrentPlayer.Hand.CityCards.Contains(cardToDiscard));
            Assert.IsTrue(game.PlayerDiscardPile.Contains(cardToDiscard));
        }

        [Test]
        public void Discard_player_card_when_no_actions_infects_cities()
        {
            var (initialGame, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });
            var game = initialGame with
            {
                Players = initialGame.Players.Replace(initialGame.CurrentPlayer, initialGame.CurrentPlayer with
                {
                    Hand = new PlayerHand(initialGame.PlayerDrawPile.TakeLast(6))
                })
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            // act
            (game, _) = game.DiscardPlayerCard(game.CurrentPlayer.Hand.First());

            Assert.AreEqual(initialGame.InfectionDrawPile.Count - 2, game.InfectionDrawPile.Count);
            Assert.AreEqual(initialGame.InfectionDiscardPile.Count + 2, game.InfectionDiscardPile.Count);
            Assert.AreEqual(TotalNumCubesOnCities(initialGame) + 2, TotalNumCubesOnCities(game));
        }

        // todo: discard another when 9 cards in hand

        [Test]
        public void Build_research_station_works()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] {Role.Medic, Role.Scientist}
            });

            var chicagoPlayerCard = new PlayerCityCard(game.Board.City("Chicago"));

            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Location = "Chicago",
                    Hand = game.CurrentPlayer.Hand.Add(chicagoPlayerCard)
                })
            };

            (game, _) = game.BuildResearchStation("Chicago");

            Assert.IsTrue(game.CityByName("Chicago").HasResearchStation);
            Assert.IsFalse(game.CurrentPlayer.Hand.Contains(chicagoPlayerCard));
            Assert.Contains(chicagoPlayerCard, game.PlayerDiscardPile);
            Assert.AreEqual(3, game.CurrentPlayer.ActionsRemaining);
        }

        [Test]
        public void Build_research_station_can_end_turn()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var chicagoPlayerCard = new PlayerCityCard(game.Board.City("Chicago"));

            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Location = "Chicago",
                    Hand = game.CurrentPlayer.Hand.Add(chicagoPlayerCard),
                    ActionsRemaining = 1
                })
            };

            AssertEndsTurn(() => game.BuildResearchStation("Chicago"));
        }

        [Test]
        public void Build_research_station_when_not_in_city_throws()
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
                    Hand = game.CurrentPlayer.Hand.Add(new PlayerCityCard(game.Board.City("Chicago")))
                })
            };

            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Chicago"));
        }

        [Test]
        public void Build_research_station_without_correct_city_card_throws()
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
                    Hand = PlayerHand.Empty
                })
            };

            Assert.AreEqual("Atlanta", game.CurrentPlayer.Location);
            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Atlanta"));
        }

        [Test]
        public void Build_research_station_where_already_exists_throws()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            var atlantaPlayerCard = new PlayerCityCard(game.Board.City("Atlanta"));

            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = game.CurrentPlayer.Hand.Add(atlantaPlayerCard)
                })
            };

            // atlanta starts with a research station
            Assert.Throws<GameRuleViolatedException>(() => game.BuildResearchStation("Atlanta"));
        }

        [Test]
        public void Cure_disease_works()
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

            (game, _) = game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray());

            Assert.IsTrue(game.CureDiscovered[Colour.Black]);
            Assert.AreEqual(0, game.CurrentPlayer.Hand.Count);
            Assert.AreEqual(5, game.PlayerDiscardPile.Count);
            Assert.AreEqual(3, game.CurrentPlayer.ActionsRemaining);
        }

        [Test]
        public void Cure_can_end_turn()
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
                    Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5)),
                    ActionsRemaining = 1
                })
            };

            AssertEndsTurn(() => game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
        }

        [Test]
        public void Cure_when_not_at_research_station_throws()
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
                    Location = "Chicago",
                    Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
                })
            };

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
        }

        [Test]
        public void Cure_when_not_enough_cards_throws()
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
                    Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(4))
                })
            };

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
        }

        [Test]
        public void Cure_with_different_colours_throws()
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
                    Hand = new PlayerHand(new []
                    {
                        new PlayerCityCard(new CityData("asdf", Colour.Black)),
                        new PlayerCityCard(new CityData("asdf", Colour.Black)),
                        new PlayerCityCard(new CityData("asdf", Colour.Black)),
                        new PlayerCityCard(new CityData("asdf", Colour.Black)),
                        new PlayerCityCard(new CityData("asdf", Colour.Blue)),
                    })
                })
            };

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
        }

        [Test]
        public void Cure_already_cured_disease_throws()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            game = game with
            {
                CureDiscovered = new Dictionary<Colour, bool>
                {
                    {Colour.Black, true},
                    {Colour.Blue, false},
                    {Colour.Red, false},
                    {Colour.Yellow, false}
                }.ToImmutableDictionary(),

                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Location = "Atlanta",
                    Hand = new PlayerHand(PlayerCards.CityCards.Where(c => c.City.Colour == Colour.Black).Take(5))
                })
            };

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DiscoverCure(game.CurrentPlayer.Hand.Cast<PlayerCityCard>().ToArray()));
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
    }
}
