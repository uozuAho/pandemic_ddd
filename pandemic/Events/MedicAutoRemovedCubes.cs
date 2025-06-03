namespace pandemic.Events;

using Values;

internal sealed record MedicAutoRemovedCubes(string City, Colour Colour) : IEvent;
