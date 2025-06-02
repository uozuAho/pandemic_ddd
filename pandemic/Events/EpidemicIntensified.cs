namespace pandemic.Events;

using System.Collections.Generic;
using Values;

public record EpidemicIntensified(IEnumerable<InfectionCard> ShuffledDiscardPile) : IEvent;
