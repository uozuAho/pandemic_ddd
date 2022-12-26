using pandemic.Values;

namespace pandemic.Commands;

public record BuildResearchStationCommand(Role Role, string City) : IPlayerCommand, IConsumesAction
{
    public override string ToString()
    {
        return $"{Role} build research station at {City}";
    }
}
