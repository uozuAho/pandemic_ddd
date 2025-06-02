namespace pandemic.Events;

using Values;

public record PlayerDroveFerried(Role Role, string Location) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: drove to {Location}";
    }
}
