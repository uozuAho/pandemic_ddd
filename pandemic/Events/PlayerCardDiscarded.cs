using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardDiscarded(PlayerCard Card) : IEvent;
}
