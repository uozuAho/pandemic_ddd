namespace pandemic.Events;

using Values;

internal sealed record EpidemicPlayerCardDiscarded(Role Role, EpidemicCard Card) : IEvent;
