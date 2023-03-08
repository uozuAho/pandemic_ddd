using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerUsedStoredCard(ISpecialEventCard Card) : IEvent;
