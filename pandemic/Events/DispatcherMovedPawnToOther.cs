using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherMovedPawnToOther(Role Role, Role DestinationRole) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: moved {Role} to {DestinationRole}";
    }
}
