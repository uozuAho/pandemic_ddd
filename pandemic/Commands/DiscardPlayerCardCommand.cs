using System;
using pandemic.Values;

namespace pandemic.Commands;

public record DiscardPlayerCardCommand(Role Role, PlayerCard Card) : IPlayerCommand
{
    public override string ToString()
    {
        return Card switch
        {
            EpidemicCard => $"{Role} discard epidemic card",
            PlayerCityCard cityCard => $"{Role} discard {cityCard.City.Name}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public bool ConsumesAction => false;
    public bool IsSpecialEvent => false;
}
