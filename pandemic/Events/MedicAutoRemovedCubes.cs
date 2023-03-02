using pandemic.Values;

namespace pandemic.Events;

internal record MedicAutoRemovedCubes(string City, Colour Colour) : IEvent;
