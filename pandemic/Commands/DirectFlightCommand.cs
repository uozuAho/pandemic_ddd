using pandemic.Values;

namespace pandemic.Commands;

/// <summary>
/// Discard a city card to fly directly to that city
/// </summary>
public record DirectFlightCommand(Role Role, string Destination) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role} direct fly to {Destination}";
    }
}
