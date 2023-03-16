using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerDroveFerried(Role Role, string Location) : IEvent
    {
        public override string ToString()
        {
            return $"{Role}: drove to {Location}";
        }
    }
}
