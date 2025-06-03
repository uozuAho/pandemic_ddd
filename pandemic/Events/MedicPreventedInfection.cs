namespace pandemic.Events;

internal sealed record MedicPreventedInfection(string City) : IEvent;
