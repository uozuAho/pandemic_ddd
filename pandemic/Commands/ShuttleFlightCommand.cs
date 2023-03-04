using pandemic.Values;

namespace pandemic.Commands;

/// <summary>
/// Move from one research station to another
/// </summary>
public record ShuttleFlightCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
