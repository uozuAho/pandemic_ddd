using pandemic.Values;

namespace pandemic.Events;

internal record ShareKnowledgeGiven(Role Role, string City, Role ReceivingRole) : IEvent;
