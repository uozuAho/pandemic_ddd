using pandemic.Values;

namespace pandemic.Events
{
    internal record EpidemicCardDiscarded(Player Player, EpidemicCard Card) : IEvent;
}
