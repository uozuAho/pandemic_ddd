using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherShuttleFlewPawn(Role PlayerToMove, string City) : IEvent;
