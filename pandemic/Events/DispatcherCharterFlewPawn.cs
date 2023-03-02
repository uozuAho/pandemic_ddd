using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherCharterFlewPawn(Role PlayerToMove, string Destination) : IEvent;
