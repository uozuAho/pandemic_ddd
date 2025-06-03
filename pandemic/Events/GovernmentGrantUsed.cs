namespace pandemic.Events;

using Values;

internal sealed record GovernmentGrantUsed(Role Role, string City) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: government grant: {City}";
    }
}
