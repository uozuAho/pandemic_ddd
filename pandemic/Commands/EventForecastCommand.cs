using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.Commands;

public record EventForecastCommand(Role Role, ImmutableList<InfectionCard> Cards)
    : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
