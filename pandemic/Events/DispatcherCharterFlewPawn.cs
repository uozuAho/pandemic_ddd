namespace pandemic.Events;

using Values;

internal sealed record DispatcherCharterFlewPawn(Role PlayerToMove, string Destination) : IEvent;
