using pandemic.Values;

namespace pandemic.Events
{
    internal record EpidemicPlayerCardDiscarded(Player Player, EpidemicCard Card) : IEvent;
}
