namespace pandemic.Events;

internal sealed record GameLost(string Reason) : IEvent;
