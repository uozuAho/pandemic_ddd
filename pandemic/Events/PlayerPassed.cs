using pandemic.Values;

namespace pandemic.Events;

public record PlayerPassed(Role Role) : IEvent;
