namespace pandemic.Events;

using Values;

internal sealed record DispatcherDroveFerriedPawn(Role Role, string City) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: drove {Role} to {City}";
    }
}
