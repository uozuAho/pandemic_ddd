using System;
using pandemic.Values;

namespace pandemic.Commands;

public record DiscardPlayerCardCommand(Role Role, PlayerCard Card) : IPlayerCommand
{
    public override string ToString()
    {
        return Card switch
        {
            EpidemicCard => "discard epidemic card",
            PlayerCityCard cityCard => $"discard {cityCard.City.Name}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
