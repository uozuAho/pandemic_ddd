namespace pandemic.Events;

using Values;

public record PlayerAdded(Role Role) : IEvent;
