using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Events;

namespace pandemic
{
    /// <summary>
    /// Keeps an up-to-date view of the game state by consuming events
    /// </summary>
    public class PandemicCurrentStateView
    {
        public Aggregates.Pandemic CurrentState => Fold(_eventLog);
        private readonly List<IEvent> _eventLog = new();

        public void SetDifficulty(Difficulty difficulty)
        {
            _eventLog.AddRange(Pandemic.SetDifficulty(_eventLog, difficulty));
        }

        private static Aggregates.Pandemic Fold(IEnumerable<IEvent> eventLog)
        {
            var initialState = new Aggregates.Pandemic();

            return eventLog.Aggregate(initialState, (current, @event) => current.Apply(@event));
        }
    }
}
