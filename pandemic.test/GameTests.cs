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
        public void Move_player(string toCity)
        {
            var eventLog = CreateNewGame();

            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, toCity));

            var state = PandemicGame.FromEvents(eventLog);
            Assert.AreEqual(toCity, state.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Move_player_to_garbage_city_throws()
        {
            var eventLog = CreateNewGame();

            Assert.Throws<InvalidActionException>(() =>
                PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "fasdfasdf").ToList());
        }

        [Test]
        public void Drive_or_ferry_to_non_adjacent_city_throws()
        {
            var eventLog = CreateNewGame();

            Assert.Throws<InvalidActionException>(() =>
                PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Beijing").ToList());
        }

        [Test]
        public void Player_draws_two_cards_after_last_action()
        {
            var eventLog = GameBuilder.InitialiseNewGame();
            var initialState = PandemicGame.FromEvents(eventLog);

            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Atlanta"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Atlanta"));

            var state = PandemicGame.FromEvents(eventLog);
            Assert.AreEqual(initialState.CurrentPlayer.Hand.Count + 2, state.CurrentPlayer.Hand.Count);
        }

        [Test]
        public void Player_attempts_fifth_action_throws()
        {
            var eventLog = GameBuilder.InitialiseNewGame();

            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Atlanta"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Atlanta"));

            Assert.Throws<GameRuleViolatedException>(() =>
                PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago").ToList());
        }

        [Test]
        [Ignore("get back to this after immutable collections")]
        public void Cities_are_infected_after_player_turn_ends()
        {
            var eventLog = GameBuilder.InitialiseNewGame();
            var startingState = PandemicGame.FromEvents(eventLog);

            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Atlanta"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));
            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Atlanta"));

            var state = PandemicGame.FromEvents(eventLog);
            Assert.AreEqual(startingState.InfectionDrawPile.Count - 2, state.InfectionDrawPile.Count);
            Assert.AreEqual(2, state.InfectionDiscardPile.Count);
        }

        private static List<IEvent> CreateNewGame()
        {
            var eventLog = new List<IEvent>();

            eventLog.AddRange(PandemicGame.SetDifficulty(eventLog, Difficulty.Normal));
            eventLog.AddRange(PandemicGame.SetInfectionRate(eventLog, 2));
            eventLog.AddRange(PandemicGame.SetOutbreakCounter(eventLog, 0));
            eventLog.AddRange(PandemicGame.AddPlayer(eventLog, Role.Medic));

            return eventLog;
        }
    }
}
