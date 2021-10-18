using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events
{
    internal record PlayerDrawPileSetupWithEpidemicCards(IEnumerable<PlayerCard> DrawPile) : IEvent;
}
