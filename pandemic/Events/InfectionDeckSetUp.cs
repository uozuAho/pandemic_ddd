using System.Collections.Generic;
using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events
{
    public record InfectionDeckSetUp : IEvent
    {
        public ImmutableList<InfectionCard> Deck { get; }

        public InfectionDeckSetUp(IEnumerable<InfectionCard> deck)
        {
            Deck = deck.ToImmutableList();
        }
    }
}
