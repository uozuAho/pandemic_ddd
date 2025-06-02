namespace pandemic.Events;

using Values;

internal record DispatcherShuttleFlewPawn(Role PlayerToMove, string City) : IEvent;
