using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerUsedStoredAirlift(Role PlayerToMove, string City) : IEvent;
