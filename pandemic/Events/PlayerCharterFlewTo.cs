namespace pandemic.Events;

using Values;

public record PlayerCharterFlewTo(Role Role, string City) : IEvent;
