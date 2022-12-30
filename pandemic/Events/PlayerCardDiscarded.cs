using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardDiscarded(Role Role, PlayerCard Card) : IEvent;
}
