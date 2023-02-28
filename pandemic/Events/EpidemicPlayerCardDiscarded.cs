using pandemic.Values;

namespace pandemic.Events
{
    internal record EpidemicPlayerCardDiscarded(Role Role, EpidemicCard Card) : IEvent;
}
