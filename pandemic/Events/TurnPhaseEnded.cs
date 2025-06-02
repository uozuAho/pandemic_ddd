namespace pandemic.Events;

using Values;

internal record TurnPhaseEnded(TurnPhase NextPhase) : IEvent;
