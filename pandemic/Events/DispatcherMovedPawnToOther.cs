using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherMovedPawnToOther(Role Role, Role DestinationRole) : IEvent;
