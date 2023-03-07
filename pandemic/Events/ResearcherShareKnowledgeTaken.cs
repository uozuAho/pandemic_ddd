using pandemic.Values;

namespace pandemic.Events;

internal record ResearcherShareKnowledgeTaken(Role Role, string City) : IEvent;
