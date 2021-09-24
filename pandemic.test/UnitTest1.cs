using System.Collections.Generic;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Events;

namespace pandemic.test
{
    public class GameSetup
    {
        [Test]
        public void Set_difficulty_works()
        {
            var eventLog = new List<IEvent>();

            eventLog.AddRange(Pandemic.SetDifficulty(eventLog, Difficulty.Normal));

            var state = Pandemic.FromEvents(eventLog);
            Assert.AreEqual(Difficulty.Normal, state.Difficulty);
        }
    }
}
