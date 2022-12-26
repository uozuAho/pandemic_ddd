using pandemic.Values;

namespace pandemic.Commands;

/// <summary>
/// Discard a city card to flight directly to that city
/// </summary>
public record DirectFlightCommand(Role Role, string Destination) : IPlayerCommand, IConsumesAction
{
    public override string ToString()
    {
        return $"{Role} direct fly to {Destination}";
    }
}
