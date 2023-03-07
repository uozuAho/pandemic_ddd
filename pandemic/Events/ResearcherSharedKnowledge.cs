using pandemic.Values;

namespace pandemic.Events;

public record ResearcherSharedKnowledge(Role PlayerToGiveTo, string City) : IEvent;
