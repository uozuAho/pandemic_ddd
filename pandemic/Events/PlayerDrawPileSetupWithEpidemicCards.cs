namespace pandemic.Events;

using System.Collections.Immutable;
using Values;

internal sealed record PlayerDrawPileSetupWithEpidemicCards(ImmutableList<PlayerCard> DrawPile) : IEvent;
