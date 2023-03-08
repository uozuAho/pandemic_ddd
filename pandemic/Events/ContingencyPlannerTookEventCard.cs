using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerTookEventCard(ISpecialEventCard Card) : IEvent;
