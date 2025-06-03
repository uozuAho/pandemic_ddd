namespace pandemic.Events;

using Values;

internal sealed record ResilientPopulationUsed(Role Role, InfectionCard InfectionCard) : IEvent;
