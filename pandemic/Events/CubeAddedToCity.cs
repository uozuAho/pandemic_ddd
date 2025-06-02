namespace pandemic.Events;

using Values;

public record CubeAddedToCity(string City, Colour Colour) : IEvent;
