using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Events;

namespace pandemic
{
    public class PandemicGame
    {
        public PandemicGameState CurrentState => Fold(_eventLog);
        private readonly List<IEvent> _eventLog = new();

        public void SetDifficulty(Difficulty difficulty)
        {
            _eventLog.AddRange(Pandemic.SetDifficulty(_eventLog, difficulty));
        }

        private static PandemicGameState Fold(IEnumerable<IEvent> eventLog)
        {
            var initialState = new PandemicGameState();

            return eventLog.Aggregate(initialState, (current, @event) => current.Apply(@event));
        }
    }
}
