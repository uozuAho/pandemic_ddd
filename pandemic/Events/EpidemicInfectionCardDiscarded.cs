namespace pandemic.Events;

using Values;

internal record EpidemicInfectionCardDiscarded(InfectionCard Card) : IEvent;
