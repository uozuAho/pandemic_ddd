namespace pandemic.Events;

using Values;

internal sealed record ContingencyPlannerUsedStoredAirlift(Role PlayerToMove, string City) : IEvent;
