using pandemic.Values;

namespace pandemic.Events;

internal record EpidemicInfectionCardDiscarded(InfectionCard Card) : IEvent;
