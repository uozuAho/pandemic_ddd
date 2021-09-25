using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardPickedUp(Role Role, PlayerCard Card) : IEvent;
}
