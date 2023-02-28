using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events;

public record EpidemicIntensified(IEnumerable<InfectionCard> ShuffledDiscardPile) : IEvent;
