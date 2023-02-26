using pandemic.Values;

namespace pandemic.Events;

public record AirliftUsed(Role Role, Role PlayerToMove, string City) : IEvent;
