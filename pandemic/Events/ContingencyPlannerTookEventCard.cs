namespace pandemic.Events;

using Values;

internal sealed record ContingencyPlannerTookEventCard(ISpecialEventCard Card) : IEvent;
