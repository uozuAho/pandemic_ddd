using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherDirectFlewPawn(Role PlayerToMove, string City) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: direct flew {PlayerToMove} to {City}";
    }
}
