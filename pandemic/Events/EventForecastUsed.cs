using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events;

internal record EventForecastUsed(Role Role, IEnumerable<InfectionCard> Cards) : IEvent;
