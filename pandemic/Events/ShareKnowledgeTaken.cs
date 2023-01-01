using pandemic.Values;

namespace pandemic.Events;

internal record ShareKnowledgeTaken(Role Role, string City, Role TakenFromRole) : IEvent;
