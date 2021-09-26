using pandemic.Values;

namespace pandemic.Events
{
    // todo: use CityData type instead?
    public record CubesAddedToCity(string City, Colour Colour) : IEvent;
}
