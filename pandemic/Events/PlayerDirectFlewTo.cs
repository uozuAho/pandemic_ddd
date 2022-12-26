using pandemic.Values;

namespace pandemic.Events;

// see DirectFlightCommand
public record PlayerDirectFlewTo(Role Role, string Destination) : IEvent;
