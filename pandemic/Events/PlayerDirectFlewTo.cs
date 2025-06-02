namespace pandemic.Events;

using Values;

// see DirectFlightCommand
public record PlayerDirectFlewTo(Role Role, string Destination) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: direct flew to {Destination}";
    }
}
