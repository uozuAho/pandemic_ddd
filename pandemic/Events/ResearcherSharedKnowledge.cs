namespace pandemic.Events;

using Values;

public record ResearcherSharedKnowledge(Role PlayerToGiveTo, string City) : IEvent;
