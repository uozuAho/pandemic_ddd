using System.Collections.Generic;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.test
{
    internal class PandemicGameImmutabilityTests
    {
        [Test]
        public void Games_from_same_events_are_not_same()
        {
            var events = new List<IEvent>();
            events.AddRange(PandemicGame.AddPlayer(events, Role.Medic));

            var game1 = PandemicGame.FromEvents(events);
            var game2 = PandemicGame.FromEvents(events);

            Assert.AreNotSame(game1, game2);
            Assert.AreNotEqual(game1, game2);
        }

        [Test]
        [Ignore("fix later")]
        public void Player_list_is_not_shallow_copy()
        {
            var events = new List<IEvent>();
            events.AddRange(PandemicGame.AddPlayer(events, Role.Medic));

            var game1 = PandemicGame.FromEvents(events);

            events.AddRange(PandemicGame.AddPlayer(events, Role.Medic));

            var game2 = PandemicGame.FromEvents(events);

            Assert.AreNotSame(game1.Players, game2.Players);
            Assert.AreNotEqual(game1.Players, game2.Players);
            Assert.AreNotSame(game1.Players[0], game2.Players[0]);
            Assert.AreEqual(game1.Players[0], game2.Players[0]);
        }

        // todo: player hands are copied
    }
}
