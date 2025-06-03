namespace pandemic.Events;

using Values;

internal sealed record ShareKnowledgeTaken(Role Role, string City, Role TakenFromRole) : IEvent;
