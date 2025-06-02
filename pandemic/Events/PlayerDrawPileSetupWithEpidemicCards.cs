namespace pandemic.Events;

using System.Collections.Immutable;
using Values;

internal record PlayerDrawPileSetupWithEpidemicCards(ImmutableList<PlayerCard> DrawPile) : IEvent;
