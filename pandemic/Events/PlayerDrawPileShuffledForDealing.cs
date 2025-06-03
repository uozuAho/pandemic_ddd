namespace pandemic.Events;

using System.Collections.Immutable;
using Values;

internal sealed record PlayerDrawPileShuffledForDealing(ImmutableList<PlayerCard> Pile) : IEvent;
