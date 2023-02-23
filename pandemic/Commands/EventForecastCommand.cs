using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Commands;

public record EventForecastCommand(Role Role, IEnumerable<InfectionCard> Cards) : IPlayerCommand;
