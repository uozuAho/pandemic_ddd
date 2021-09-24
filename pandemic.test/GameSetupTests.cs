using System.Collections.Generic;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.test
{
    public class GameSetup
    {
        [Test]
        public void Do_all_the_stuff_to_start_a_game()
        {
            var eventLog = new List<IEvent>();

            eventLog.AddRange(PandemicGame.SetDifficulty(eventLog, Difficulty.Normal));
            eventLog.AddRange(PandemicGame.SetInfectionRate(eventLog, 2));
            eventLog.AddRange(PandemicGame.SetOutbreakCounter(eventLog, 0));
            eventLog.AddRange(PandemicGame.SetupInfectionDeck(eventLog));
            eventLog.AddRange(PandemicGame.AddPlayer(eventLog, Role.Medic));

            var state = PandemicGame.FromEvents(eventLog);
            Assert.AreEqual(Difficulty.Normal, state.Difficulty);
            Assert.AreEqual(2, state.InfectionRate);
            Assert.AreEqual(0, state.OutbreakCounter);
            Assert.AreEqual(1, state.Players.Count);
            Assert.AreEqual(48, state.InfectionDrawPile.Count);
            Assert.AreEqual(0, state.InfectionDiscardPile.Count);
        }
    }
}
