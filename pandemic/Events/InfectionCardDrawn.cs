using pandemic.GameData;

namespace pandemic.Events
{
    public record InfectionCardDrawn(CityData City) : IEvent;
}
