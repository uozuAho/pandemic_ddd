using System;
using pandemic.Events;

namespace pandemic.Aggregates
{
    public record PandemicGameState
    {
        public Difficulty Difficulty { get; init; }

        public PandemicGameState Apply(IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => this with {Difficulty = d.Difficulty},
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }
    }
}
