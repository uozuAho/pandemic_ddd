namespace pandemic.Commands;

using Values;

public record BuildResearchStationCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role} build research station at {City}";
    }
}
