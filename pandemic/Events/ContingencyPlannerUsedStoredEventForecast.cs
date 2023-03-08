using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerUsedStoredEventForecast(ImmutableList<InfectionCard> Cards) : IEvent;
