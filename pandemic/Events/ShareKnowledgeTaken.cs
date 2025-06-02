namespace pandemic.Events;

using Values;

internal record ShareKnowledgeTaken(Role Role, string City, Role TakenFromRole) : IEvent;
