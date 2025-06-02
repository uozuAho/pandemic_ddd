namespace pandemic.Events;

using System.Collections.Immutable;
using Values;

public record InfectionDeckSetUp(ImmutableList<InfectionCard> Deck) : IEvent;
