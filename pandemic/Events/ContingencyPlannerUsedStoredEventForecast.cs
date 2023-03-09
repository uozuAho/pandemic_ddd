using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Events;

internal record ContingencyPlannerUsedStoredEventForecast(IReadOnlyList<InfectionCard> Cards) : IEvent;
