using pandemic.Values;

namespace pandemic.Events;

public record PlayerShuttleFlewTo(Role Role, string City) : IEvent;
