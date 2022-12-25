using pandemic.Values;

namespace pandemic.Commands;

/// <summary>
/// Discard the card matching the city you're in to fly to any city
/// </summary>
public record CharterFlightCommand(Role Role, PlayerCityCard DiscardedCard, string Destination) : PlayerCommand
{
    public override string ToString()
    {
        return $"{Role} discards {DiscardedCard} to charter flight to {Destination}";
    }
}
