using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerUsedStoredResilientPopulation(InfectionCard InfectionCard) : IEvent;
