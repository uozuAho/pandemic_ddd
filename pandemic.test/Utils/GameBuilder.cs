using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.test.Utils
{
    internal class GameBuilder
    {
        public static List<IEvent> InitialiseNewGame()
        {
            var eventLog = new List<IEvent>();

            eventLog.AddRange(PandemicGame.SetDifficulty(eventLog, Difficulty.Normal));
            eventLog.AddRange(PandemicGame.SetInfectionRate(eventLog, 2));
            eventLog.AddRange(PandemicGame.SetOutbreakCounter(eventLog, 0));
            eventLog.AddRange(PandemicGame.SetupInfectionDeck(eventLog));
            eventLog.AddRange(PandemicGame.AddPlayer(eventLog, Role.Medic));

            return eventLog;
        }
    }
}
