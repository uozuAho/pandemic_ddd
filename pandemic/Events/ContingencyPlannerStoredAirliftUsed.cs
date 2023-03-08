using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerStoredAirliftUsed(Role PlayerToMove, string City) : IEvent;
