namespace pandemic.Commands;

using System.Collections.Immutable;
using System.Linq;
using Values;

public record DiscoverCureCommand(Role Role, ImmutableArray<PlayerCityCard> Cards) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"Cure {Cards.First().City.Colour}";
    }
}
