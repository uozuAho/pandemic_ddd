using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events
{
    internal record PlayerDrawPileSetupWithEpidemicCards(ImmutableList<PlayerCard> DrawPile) : IEvent;
}
