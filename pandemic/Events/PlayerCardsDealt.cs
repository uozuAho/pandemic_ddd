namespace pandemic.Events;

using Values;

internal record PlayerCardsDealt(Role Role, PlayerCard[] Cards) : IEvent;
