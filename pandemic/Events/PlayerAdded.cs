using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerAdded : IEvent
    {
        public Role Role { get; init; }

        public PlayerAdded(Role role)
        {
            Role = role;
        }
    }
}
