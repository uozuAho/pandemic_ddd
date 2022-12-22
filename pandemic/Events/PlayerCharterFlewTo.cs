using pandemic.Values;

namespace pandemic.Events;

public record PlayerCharterFlewTo(Role Role, string City) : IEvent;
