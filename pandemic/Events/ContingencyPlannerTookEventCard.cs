namespace pandemic.Events;

using Values;

internal record ContingencyPlannerTookEventCard(ISpecialEventCard Card) : IEvent;
