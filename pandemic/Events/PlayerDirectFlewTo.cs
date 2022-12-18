using pandemic.Values;

namespace pandemic.Events;

public record PlayerDirectFlewTo(Role Role, string City) : IEvent;
