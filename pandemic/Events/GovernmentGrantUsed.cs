using pandemic.Values;

namespace pandemic.Events;

internal record GovernmentGrantUsed(Role Role, string City) : IEvent;
