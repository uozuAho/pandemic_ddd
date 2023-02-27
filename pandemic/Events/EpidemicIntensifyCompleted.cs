﻿using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events;

public record EpidemicIntensifyCompleted(IEnumerable<InfectionCard> ShuffledDiscardPile) : IEvent;
