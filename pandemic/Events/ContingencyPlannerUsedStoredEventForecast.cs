namespace pandemic.Events;

using System.Collections.Generic;
using Values;

internal sealed record ContingencyPlannerUsedStoredEventForecast(IReadOnlyList<InfectionCard> Cards)
    : IEvent;
