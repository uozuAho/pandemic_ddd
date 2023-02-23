using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events;

public record EpidemicInfectionDiscardPileShuffledAndReplaced(IEnumerable<InfectionCard> ShuffledDiscardPile) : IEvent;
