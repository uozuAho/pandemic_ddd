using pandemic.Values;

namespace pandemic.Events;

internal record ResilientPopulationUsed(Role Role, InfectionCard InfectionCard) : IEvent;
