using pandemic.GameData;

namespace pandemic.Events
{
    public record CubeAddedToCity(CityData City) : IEvent;
}
