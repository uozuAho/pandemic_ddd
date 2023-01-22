using pandemic.Values;

namespace pandemic.Events;

public record DiseaseEradicated(Colour Colour) : IEvent;
