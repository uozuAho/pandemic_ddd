namespace pandemic.Commands;

using Values;

/// <summary>
/// Discard the card matching the city you're in to fly to any city
/// </summary>
public record CharterFlightCommand(Role Role, PlayerCityCard DiscardCard, string Destination)
    : IPlayerCommand
{
    public override string ToString()
    {
        return $"{Role} discards {DiscardCard.City.Name} to charter flight to {Destination}";
    }

    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
