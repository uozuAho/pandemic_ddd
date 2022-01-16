namespace pandemic.Commands;

public record BuildResearchStationCommand(string City) : PlayerCommand
{
    public override string ToString()
    {
        return $"build research station at {City}";
    }
}
