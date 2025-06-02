namespace pandemic.Events;

using System.Collections.Generic;
using Values;

internal record EventForecastUsed(Role Role, IReadOnlyList<InfectionCard> Cards) : IEvent;
