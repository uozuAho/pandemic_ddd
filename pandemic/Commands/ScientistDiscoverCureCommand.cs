namespace pandemic.Commands;

using System.Collections.Immutable;
using Values;

public record ScientistDiscoverCureCommand(ImmutableArray<PlayerCityCard> Cards) : IPlayerCommand
{
    public Role Role => Role.Scientist;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
