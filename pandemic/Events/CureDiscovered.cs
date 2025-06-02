namespace pandemic.Events;

using Values;

public record CureDiscovered(Colour Colour) : IEvent;
