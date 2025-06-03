namespace pandemic.Events;

using Values;

internal sealed record EpidemicInfectionCardDiscarded(InfectionCard Card) : IEvent;
