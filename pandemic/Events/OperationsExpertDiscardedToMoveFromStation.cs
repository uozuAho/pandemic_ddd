namespace pandemic.Events;

internal sealed record OperationsExpertDiscardedToMoveFromStation(string DiscardedCard, string Destination)
    : IEvent;
