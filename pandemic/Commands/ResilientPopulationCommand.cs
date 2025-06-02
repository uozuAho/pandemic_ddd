namespace pandemic.Commands;

using Values;

public record ResilientPopulationCommand(Role Role, InfectionCard Card) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
