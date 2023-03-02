using pandemic.Values;

namespace pandemic.Events;

internal record MedicTreatedDisease(string City, Colour Colour) : IEvent;
