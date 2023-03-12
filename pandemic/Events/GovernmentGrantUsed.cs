using pandemic.Values;

namespace pandemic.Events;

internal record GovernmentGrantUsed(Role Role, string City) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: government grant: {City}";
    }
}
