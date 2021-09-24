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

        // create a game state from an event log
        public static PandemicGame FromEvents(IEnumerable<IEvent> events) =>
            events.Aggregate(new PandemicGame(), Apply);

        // commands yield events
        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }

        // Game state is modified by events. Return a new object instead of mutating.
        public static PandemicGame Apply(PandemicGame pandemicGame, IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => pandemicGame with {Difficulty = d.Difficulty},
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }
    }
}
