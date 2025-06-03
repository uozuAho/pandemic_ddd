namespace pandemic.Events;

using Values;

internal sealed record ResearcherShareKnowledgeTaken(Role Role, string City) : IEvent;
