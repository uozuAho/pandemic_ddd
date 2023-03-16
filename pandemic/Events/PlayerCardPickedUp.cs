using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardPickedUp(Role Role, PlayerCard Card) : IEvent
    {
        public override string ToString()
        {
            return $"{Role}: picked up {Card}";
        }
    }
}
