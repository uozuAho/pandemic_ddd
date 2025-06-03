namespace pandemic.Events;

using Values;

internal sealed record ContingencyPlannerUsedStoredResilientPopulation(InfectionCard InfectionCard)
    : IEvent;
