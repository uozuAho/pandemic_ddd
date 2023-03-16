using pandemic.Values;

namespace pandemic.Events
{
    public record InfectionCardDrawn(InfectionCard Card) : IEvent
    {
        public override string ToString()
        {
            return $"Infection card: {Card.City} ({Card.Colour})";
        }
    }
}
