using pandemic.Values;

namespace pandemic.Commands;

public record BuildResearchStationCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;

    public override string ToString()
    {
        return $"{Role} build research station at {City}";
    }
}
