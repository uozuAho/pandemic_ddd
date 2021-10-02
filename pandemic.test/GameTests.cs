using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.test.Utils;
using pandemic.Values;

namespace pandemic.test
{
    internal class GameTests
    {
        [TestCase("Chicago")]
        [TestCase("Washington")]
        public void Drive_or_ferry_player(string toCity)
        {
            var game = GameBuilder.InitialiseNewGame();

            (game, _) = game.DriveOrFerryPlayer(Role.Medic, toCity);

            Assert.AreEqual(toCity, game.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Drive_or_ferry_to_garbage_city_throws()
        {
            var game = GameBuilder.InitialiseNewGame();

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(Role.Medic, "fasdfasdf"));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var game = GameBuilder.InitialiseNewGame();

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(Role.Medic, "Beijing"));
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
        {
            var startingState = GameBuilder.InitialiseNewGame();

            var (game, _) = startingState.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Chicago");
            (game, _) = game.DriveOrFerryPlayer(Role.Medic, "Atlanta");

            Assert.AreEqual(startingState.CurrentPlayer.Hand.Count + 2, game.CurrentPlayer.Hand.Count);
        }

        [Test]
        public void Player_attempts_fifth_action_throws()
        {
            var game = GameBuilder.InitialiseNewGame();

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
            var startingState = GameBuilder.InitialiseNewGame();

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
            var game = GameBuilder.InitialiseNewGame() with
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
        public void Next_player_turn_after_infect_cities()
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
    }
}
