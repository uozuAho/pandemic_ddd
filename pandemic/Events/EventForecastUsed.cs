using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events;

internal record EventForecastUsed(Role Role, ImmutableList<InfectionCard> Cards) : IEvent;
