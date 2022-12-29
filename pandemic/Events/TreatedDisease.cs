using pandemic.Values;

namespace pandemic.Events;

internal record TreatedDisease(Role Role, string City, Colour Colour) : IEvent;
