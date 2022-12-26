using pandemic.Values;

namespace pandemic.Commands;

/// <summary>
/// Discard the card matching the city you're in to fly to any city
/// </summary>
public record CharterFlightCommand(Role Role, PlayerCityCard DiscardCard, string Destination) :
    PlayerCommand,
    IConsumesAction
{
    public override string ToString()
    {
        return $"{Role} discards {DiscardCard.City.Name} to charter flight to {Destination}";
    }
}
