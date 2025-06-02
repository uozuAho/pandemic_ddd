namespace pandemic.Events;

using Values;

internal record DispatcherDroveFerriedPawn(Role Role, string City) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: drove {Role} to {City}";
    }
}
