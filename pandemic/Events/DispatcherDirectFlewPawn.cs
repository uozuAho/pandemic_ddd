namespace pandemic.Events;

using Values;

internal sealed record DispatcherDirectFlewPawn(Role PlayerToMove, string City) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: direct flew {PlayerToMove} to {City}";
    }
}
