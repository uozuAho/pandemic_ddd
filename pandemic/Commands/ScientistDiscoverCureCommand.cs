using pandemic.Values;

namespace pandemic.Commands;

public record ScientistDiscoverCureCommand(PlayerCityCard[] Cards) : IPlayerCommand
{
    public Role Role => Role.Scientist;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
