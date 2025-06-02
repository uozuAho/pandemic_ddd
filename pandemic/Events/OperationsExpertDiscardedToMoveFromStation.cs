namespace pandemic.Events;

internal record OperationsExpertDiscardedToMoveFromStation(string DiscardedCard, string Destination)
    : IEvent;
