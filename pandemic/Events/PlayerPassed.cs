namespace pandemic.Events;

using Values;

public record PlayerPassed(Role Role) : IEvent;
