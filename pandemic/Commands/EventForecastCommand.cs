using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Commands;

public record EventForecastCommand(Role Role, ImmutableList<InfectionCard> Cards)
    : IPlayerCommand, ISpecialEventCommand;
