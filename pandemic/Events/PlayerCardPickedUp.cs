using pandemic.Values;

namespace pandemic.Events
{
    // todo: remove role?
    public record PlayerCardPickedUp(Role Role) : IEvent;
}
