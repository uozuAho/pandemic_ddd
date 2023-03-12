using pandemic.Values;

namespace pandemic.Events
{
    public record ResearchStationBuilt(Role Role, string City) : IEvent
    {
        public override string ToString()
        {
            return $"{Role}: built research station in {City}";
        }
    }
}
