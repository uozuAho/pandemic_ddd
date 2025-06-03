namespace pandemic.Events;

using System.Collections.Generic;
using Values;

internal sealed record EventForecastUsed(Role Role, IReadOnlyList<InfectionCard> Cards) : IEvent;
