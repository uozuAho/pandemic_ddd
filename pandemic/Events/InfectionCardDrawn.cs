using pandemic.Values;

namespace pandemic.Events
{
    public record InfectionCardDrawn(string City, Colour Colour) : IEvent;
}
