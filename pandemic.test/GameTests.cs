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
            var eventLog = CreateNewGame();
            var state = PandemicGame.FromEvents(eventLog);

            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, toCity);

            Assert.AreEqual(toCity, state.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Drive_or_ferry_to_garbage_city_throws()
        {
            var eventLog = CreateNewGame();
            var state = PandemicGame.FromEvents(eventLog);

            Assert.Throws<InvalidActionException>(() =>
                PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "fasdfasdf"));
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var eventLog = CreateNewGame();
            var state = PandemicGame.FromEvents(eventLog);

            Assert.Throws<InvalidActionException>(() =>
                PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Beijing"));
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
        {
            var eventLog = GameBuilder.InitialiseNewGame();
            var startingState = PandemicGame.FromEvents(eventLog);

            var state = PandemicGame.DriveOrFerryPlayer(startingState, eventLog, Role.Medic, "Chicago");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Atlanta");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Chicago");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Atlanta");

            Assert.AreEqual(startingState.CurrentPlayer.Hand.Count + 2, state.CurrentPlayer.Hand.Count);
        }

        [Test]
        public void Player_attempts_fifth_action_throws()
        {
            var eventLog = GameBuilder.InitialiseNewGame();
            var state = PandemicGame.FromEvents(eventLog);

            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Chicago");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Atlanta");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Chicago");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Atlanta");

            Assert.Throws<GameRuleViolatedException>(() =>
                PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Chicago"));
        }

        [Test]
        public void Cities_are_infected_after_player_turn_ends()
        {
            var eventLog = GameBuilder.InitialiseNewGame();
            var startingState = PandemicGame.FromEvents(eventLog);

            var state = PandemicGame.DriveOrFerryPlayer(startingState, eventLog, Role.Medic, "Chicago");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Atlanta");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Chicago");
            state = PandemicGame.DriveOrFerryPlayer(state, eventLog, Role.Medic, "Atlanta");

            Assert.AreEqual(startingState.InfectionDrawPile.Count - 2, state.InfectionDrawPile.Count);
            Assert.AreEqual(2, state.InfectionDiscardPile.Count);

            foreach (var infectionCard in state.InfectionDiscardPile.TakeLast(2))
            {
                Assert.AreEqual(1, state.CityByName(infectionCard.City.Name).Cubes[infectionCard.City.Colour]);
            }
        }

        private static List<IEvent> CreateNewGame()
        {
            var eventLog = new List<IEvent>();
            var game = PandemicGame.FromEvents(eventLog);

            game = game.SetDifficulty(eventLog, Difficulty.Normal);
            game = game.SetInfectionRate(eventLog, 2);
            game = game.SetOutbreakCounter(eventLog, 0);
            game = game.AddPlayer(eventLog, Role.Medic);

            return eventLog;
        }
    }
}
