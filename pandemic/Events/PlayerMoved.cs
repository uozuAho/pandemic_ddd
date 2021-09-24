using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerMoved : IEvent
    {
        public Role Role { get; init; }
        public string Location { get; init; }

        public PlayerMoved(Role role, string city)
        {
            Role = role;
            Location = city;
        }
    }
}
