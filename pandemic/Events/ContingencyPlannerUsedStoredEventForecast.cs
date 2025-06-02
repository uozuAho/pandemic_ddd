namespace pandemic.Events;

using System.Collections.Generic;
using Values;

internal record ContingencyPlannerUsedStoredEventForecast(IReadOnlyList<InfectionCard> Cards)
    : IEvent;
