namespace pandemic.Events;

using Values;

internal record DispatcherCharterFlewPawn(Role PlayerToMove, string Destination) : IEvent;
