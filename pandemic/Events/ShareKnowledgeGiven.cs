namespace pandemic.Events;

using Values;

internal sealed record ShareKnowledgeGiven(Role Role, string City, Role ReceivingRole) : IEvent;
