using pandemic.Values;

namespace pandemic.Events
{
    public record CubeAddedToCity(string City, Colour Colour) : IEvent;
}
