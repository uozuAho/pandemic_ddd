using pandemic.Values;

namespace pandemic.Events
{
    // todo: use CityData type instead?
    public record CubeAddedToCity(string City, Colour Colour) : IEvent;
}
