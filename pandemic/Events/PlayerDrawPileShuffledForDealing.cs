using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events
{
    internal record PlayerDrawPileShuffledForDealing(ImmutableList<PlayerCard> Pile) : IEvent;
}
