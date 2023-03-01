using pandemic.Values;

namespace pandemic.Commands;

public record ResilientPopulationCommand(Role Role, InfectionCard Card) : IPlayerCommand, ISpecialEventCommand
{
    public bool ConsumesAction => false;
}
