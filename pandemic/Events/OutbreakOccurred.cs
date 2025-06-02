namespace pandemic.Events;

public record OutbreakOccurred(string City) : IEvent;
