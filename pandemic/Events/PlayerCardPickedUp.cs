using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardPickedUp(PlayerCard Card) : IEvent;
}
