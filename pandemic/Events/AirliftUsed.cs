using pandemic.Values;

namespace pandemic.Events;

public record AirliftUsed(Role Role, string City) : IEvent;
