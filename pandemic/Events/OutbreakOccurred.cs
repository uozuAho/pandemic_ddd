namespace pandemic.Events;

internal record OutbreakOccurred(string City) : IEvent;
