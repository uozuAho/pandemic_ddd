namespace pandemic.Commands;

public record BuildResearchStationCommand(string City) : PlayerCommand, IConsumesAction
{
    public override string ToString()
    {
        return $"build research station at {City}";
    }
}
