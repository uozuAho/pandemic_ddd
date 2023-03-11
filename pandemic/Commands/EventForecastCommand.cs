using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Commands;

public record EventForecastCommand(Role Role, IReadOnlyList<InfectionCard> Cards)
    : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
