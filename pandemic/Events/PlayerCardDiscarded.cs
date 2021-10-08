using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardDiscarded(Role Role, string City) : IEvent;
}
