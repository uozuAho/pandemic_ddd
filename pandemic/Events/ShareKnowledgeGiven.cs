namespace pandemic.Events;

using Values;

internal record ShareKnowledgeGiven(Role Role, string City, Role ReceivingRole) : IEvent;
