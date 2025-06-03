namespace pandemic.Events;

using Values;

internal sealed record PlayerCardsDealt(Role Role, PlayerCard[] Cards) : IEvent;
