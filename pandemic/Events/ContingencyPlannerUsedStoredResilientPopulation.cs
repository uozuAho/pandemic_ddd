namespace pandemic.Events;

using Values;

internal record ContingencyPlannerUsedStoredResilientPopulation(InfectionCard InfectionCard)
    : IEvent;
