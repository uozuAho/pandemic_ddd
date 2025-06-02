namespace pandemic.Events;

using Values;

public record PlayerShuttleFlewTo(Role Role, string City) : IEvent;
