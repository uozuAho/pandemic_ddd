namespace pandemic.Events;

using System.Collections.Immutable;
using Values;

internal record PlayerDrawPileShuffledForDealing(ImmutableList<PlayerCard> Pile) : IEvent;
