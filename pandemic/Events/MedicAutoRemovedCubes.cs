namespace pandemic.Events;

using Values;

internal record MedicAutoRemovedCubes(string City, Colour Colour) : IEvent;
