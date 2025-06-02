namespace pandemic.Events;

using Values;

internal record ContingencyPlannerUsedStoredAirlift(Role PlayerToMove, string City) : IEvent;
