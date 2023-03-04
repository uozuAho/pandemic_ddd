using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerDroveFerried(Role Role, string Location): IEvent;
}
