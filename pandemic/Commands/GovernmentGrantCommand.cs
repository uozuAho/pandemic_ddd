using pandemic.Values;

namespace pandemic.Commands;

public record GovernmentGrantCommand(Role Role, string City) : IPlayerCommand, ISpecialEventCommand
{
    public bool ConsumesAction => false;
}
