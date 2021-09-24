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
        public int InfectionRate { get; init; }
        public int OutbreakCounter { get; set; }

        public static PandemicGame FromEvents(IEnumerable<IEvent> events) =>
            events.Aggregate(new PandemicGame(), Apply);

        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }

        public static IEnumerable<IEvent> SetInfectionRate(List<IEvent> log, int rate)
        {
            yield return new InfectionRateSet(rate);
        }

        public static IEnumerable<IEvent> SetOutbreakCounter(List<IEvent> log, int value)
        {
            yield return new OutbreakCounterSet(value);
        }

        public static PandemicGame Apply(PandemicGame pandemicGame, IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => pandemicGame with {Difficulty = d.Difficulty},
                InfectionRateSet i => pandemicGame with {InfectionRate = i.Rate},
                OutbreakCounterSet o => pandemicGame with {OutbreakCounter = o.Value},
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }
    }
}
