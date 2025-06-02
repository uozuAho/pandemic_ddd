namespace pandemic.Events;

using Values;

internal record ResilientPopulationUsed(Role Role, InfectionCard InfectionCard) : IEvent;
