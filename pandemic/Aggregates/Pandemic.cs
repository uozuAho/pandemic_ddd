using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Events;

namespace pandemic.Aggregates
{
    public record Pandemic
    {
        public Difficulty Difficulty { get; init; }

        public static Pandemic FromEvents(IEnumerable<IEvent> events) => Fold(events);

        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }

        public Pandemic Apply(IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => this with {Difficulty = d.Difficulty},
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }

        private static Pandemic Fold(IEnumerable<IEvent> eventLog)
        {
            var initialState = new Pandemic();

            return eventLog.Aggregate(initialState, (current, @event) => current.Apply(@event));
        }
    }
}
