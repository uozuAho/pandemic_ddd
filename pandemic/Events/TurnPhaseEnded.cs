namespace pandemic.Events;

using Values;

internal sealed record TurnPhaseEnded(TurnPhase NextPhase) : IEvent;
