using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events
{
    internal record PlayerDrawPileSetUp(ImmutableList<PlayerCard> Pile) : IEvent;
}
