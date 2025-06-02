namespace pandemic.Commands;

using System.Collections.Generic;
using Values;

public record EventForecastCommand(Role Role, IReadOnlyList<InfectionCard> Cards) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
