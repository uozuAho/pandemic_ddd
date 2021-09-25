using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events
{
    public record InfectionDeckSetUp(ImmutableList<InfectionCard> Deck) : IEvent;
}
