using pandemic.Values;

namespace pandemic.Commands;

public record BuildResearchStationCommand(Role Role, string City) : PlayerCommand, IConsumesAction
{
    public override string ToString()
    {
        return $"{Role} build research station at {City}";
    }
}
