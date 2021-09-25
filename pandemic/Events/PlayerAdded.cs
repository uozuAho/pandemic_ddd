using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerAdded (Role Role) : IEvent;
}
