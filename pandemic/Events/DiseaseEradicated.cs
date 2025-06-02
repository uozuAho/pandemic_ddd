namespace pandemic.Events;

using Values;

public record DiseaseEradicated(Colour Colour) : IEvent;
