using System;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
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
                    Hand = ImmutableList.Create(Enumerable.Repeat(new PlayerCityCard("asdf") as PlayerCard, 7).ToArray())
                })
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.AreEqual(Role.Medic, game.CurrentPlayer.Role);
            Assert.AreEqual(0, game.CurrentPlayer.ActionsRemaining);
            Assert.True(new PlayerMoveGenerator().LegalMoves(game).All(move => move is DiscardPlayerCardCommand));
        }

        [Test]
        public void Cities_are_infected_after_player_discards()
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
                    Hand = game.PlayerDrawPile.TakeLast(6).ToImmutableList()
                })
            };

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            // todo: discard any card here. this is a temp workaround around half-implemented code
            var toDiscard = game.PlayerByRole(Role.Medic).Hand.First(c => c.City != "");
            (game, _) = game.DiscardPlayerCard(toDiscard);
        }

        // todo: cities infected after player discards
        // todo: discarded card goes to pile
    }
}
