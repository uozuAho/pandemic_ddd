using System.Linq;
using pandemic.Values;

namespace pandemic.Commands;

public record DiscoverCureCommand(Role Role, PlayerCityCard[] Cards) : IPlayerCommand, IConsumesAction
{
    public override string ToString()
    {
        return $"Cure {Cards.First().City.Colour}";
    }
}
