using System.Collections.Generic;
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

            game = game.DriveOrFerryPlayer(new List<IEvent>(), Role.Medic, toCity);

            Assert.AreEqual(toCity, game.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Drive_or_ferry_to_garbage_city_throws()
        {
            var game = GameBuilder.InitialiseNewGame();

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(new List<IEvent>(), Role.Medic, "fasdfasdf"));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var game = GameBuilder.InitialiseNewGame();

            Assert.Throws<InvalidActionException>(() =>
                game.DriveOrFerryPlayer(new List<IEvent>(), Role.Medic, "Beijing"));
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
        {
            var startingState = GameBuilder.InitialiseNewGame();
            var events = new List<IEvent>();

            var game = startingState.DriveOrFerryPlayer(events, Role.Medic, "Chicago");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Atlanta");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Chicago");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Atlanta");

            Assert.AreEqual(startingState.CurrentPlayer.Hand.Count + 2, game.CurrentPlayer.Hand.Count);
        }

        [Test]
        public void Player_attempts_fifth_action_throws()
        {
            var game = GameBuilder.InitialiseNewGame();
            var events = new List<IEvent>();

            game = game.DriveOrFerryPlayer(events, Role.Medic, "Chicago");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Atlanta");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Chicago");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Atlanta");

            Assert.Throws<GameRuleViolatedException>(() =>
                game.DriveOrFerryPlayer(events, Role.Medic, "Chicago"));
        }

        [Test]
        public void Cities_are_infected_after_player_turn_ends()
        {
            var startingState = GameBuilder.InitialiseNewGame();
            var events = new List<IEvent>();

            var game = startingState.DriveOrFerryPlayer(events, Role.Medic, "Chicago");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Atlanta");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Chicago");
            game = game.DriveOrFerryPlayer(events, Role.Medic, "Atlanta");

            Assert.AreEqual(startingState.InfectionDrawPile.Count - 2, game.InfectionDrawPile.Count);
            Assert.AreEqual(2, game.InfectionDiscardPile.Count);

            foreach (var infectionCard in game.InfectionDiscardPile.TakeLast(2))
            {
                Assert.AreEqual(1, game.CityByName(infectionCard.City.Name).Cubes[infectionCard.City.Colour]);
            }
        }
    }
}
