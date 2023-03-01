using pandemic.Values;

namespace pandemic.Commands;

public record ResilientPopulationCommand(Role Role, InfectionCard Card) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
