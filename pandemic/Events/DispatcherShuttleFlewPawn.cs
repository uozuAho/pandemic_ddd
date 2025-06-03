namespace pandemic.Events;

using Values;

internal sealed record DispatcherShuttleFlewPawn(Role PlayerToMove, string City) : IEvent;
