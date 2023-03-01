using pandemic.Values;

namespace pandemic.Commands;

public record AirliftCommand(Role Role, Role PlayerToMove, string City) : IPlayerCommand, ISpecialEventCommand
{
    public bool ConsumesAction => false;
}
