using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherDirectFlewPawn(Role PlayerToMove, string City) : IEvent;
