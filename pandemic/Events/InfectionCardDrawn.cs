using pandemic.Values;

namespace pandemic.Events
{
    public record InfectionCardDrawn(InfectionCard Card) : IEvent;
}
