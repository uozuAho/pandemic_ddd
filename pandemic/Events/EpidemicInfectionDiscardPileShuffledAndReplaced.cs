using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events;

internal record EpidemicInfectionDiscardPileShuffledAndReplaced(IEnumerable<InfectionCard> ShuffledDiscardPile) : IEvent;
