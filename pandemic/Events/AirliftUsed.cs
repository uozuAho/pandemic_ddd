namespace pandemic.Events;

using Values;

public record AirliftUsed(Role Role, Role PlayerToMove, string City) : IEvent;
