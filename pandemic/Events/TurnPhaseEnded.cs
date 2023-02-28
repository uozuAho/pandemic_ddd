using pandemic.Values;

namespace pandemic.Events;

internal record TurnPhaseEnded(TurnPhase NextPhase) : IEvent;
