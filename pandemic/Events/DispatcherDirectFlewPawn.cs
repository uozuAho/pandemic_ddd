namespace pandemic.Events;

using Values;

internal record DispatcherDirectFlewPawn(Role PlayerToMove, string City) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: direct flew {PlayerToMove} to {City}";
    }
}
