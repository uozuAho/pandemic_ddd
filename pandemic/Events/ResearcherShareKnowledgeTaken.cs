namespace pandemic.Events;

using Values;

internal record ResearcherShareKnowledgeTaken(Role Role, string City) : IEvent;
