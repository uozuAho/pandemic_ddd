using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.test
{
    internal class GameTests
    {
        [Test]
        public void Move_player()
        {
            var eventLog = CreateNewGame();

            eventLog.AddRange(PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "Chicago"));

            var state = PandemicGame.FromEvents(eventLog);
            Assert.AreEqual("Chicago", state.PlayerByRole(Role.Medic).Location);
        }

        [Test]
        public void Move_player_to_garbage_city_throws()
        {
            var eventLog = CreateNewGame();

            Assert.Throws<InvalidActionException>(() =>
                PandemicGame.DriveOrFerryPlayer(eventLog, Role.Medic, "fasdfasdf").ToList());
        }

        // todo: invalid move
        // todo: player dict is cloned

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
