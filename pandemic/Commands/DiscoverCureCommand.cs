using System.Linq;
using pandemic.Values;

namespace pandemic.Commands;

public record DiscoverCureCommand(PlayerCityCard[] Cards) : PlayerCommand
{
    public override string ToString()
    {
        return $"Cure {Cards.First().City.Colour}";
    }
}
