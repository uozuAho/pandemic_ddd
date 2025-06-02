namespace pandemic.Events;

using Values;

internal record EpidemicPlayerCardDiscarded(Role Role, EpidemicCard Card) : IEvent;
