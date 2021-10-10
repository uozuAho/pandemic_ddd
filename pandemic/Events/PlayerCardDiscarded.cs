using pandemic.Values;

namespace pandemic.Events
{
    // todo: change city to player card
    public record PlayerCardDiscarded(Role Role, string City) : IEvent;
}
