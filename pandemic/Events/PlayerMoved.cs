using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerMoved(Role Role, string Location): IEvent;
}
