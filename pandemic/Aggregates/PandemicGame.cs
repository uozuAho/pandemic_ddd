using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates
{
    public record PandemicGame
    {
        public Difficulty Difficulty { get; init; }

        // create the aggregate from an event log
        public static PandemicGame FromEvents(IEnumerable<IEvent> events) => Fold(events);

        // command
        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }

        // Modify state with events. Return a new object instead of mutating.
        public static PandemicGame Apply(PandemicGame pandemicGame, IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => pandemicGame with {Difficulty = d.Difficulty},
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }

        // convenience method that applies all events to get the current state
        private static PandemicGame Fold(IEnumerable<IEvent> eventLog)
        {
            var initialState = new PandemicGame();

            return eventLog.Aggregate(initialState, Apply);
        }
    }
}
