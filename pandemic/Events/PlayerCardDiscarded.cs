using pandemic.Values;

namespace pandemic.Events
{
    public record PlayerCardDiscarded(Role Role, PlayerCard Card) : IEvent
    {
        public override string ToString()
        {
            return $"{Role}: discarded {Card}";
        }
    }
}
