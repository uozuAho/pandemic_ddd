namespace pandemic.Events;

internal record GameLost(string Reason) : IEvent;
