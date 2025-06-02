namespace pandemic.Commands;

using Values;

public record DiscardPlayerCardCommand(Role Role, PlayerCard Card) : IPlayerCommand
{
    public override string ToString()
    {
        return Card switch
        {
            EpidemicCard => $"{Role} discard epidemic card",
            PlayerCityCard cityCard => $"{Role} discard {cityCard.City.Name}",
            _ => "ERROR: Card must be EpidemicCard or PlayerCityCard",
        };
    }

    public bool ConsumesAction => false;
    public bool IsSpecialEvent => false;
}
