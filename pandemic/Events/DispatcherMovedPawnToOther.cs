namespace pandemic.Events;

using Values;

internal sealed record DispatcherMovedPawnToOther(Role Role, Role DestinationRole) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: moved {Role} to {DestinationRole}";
    }
}
