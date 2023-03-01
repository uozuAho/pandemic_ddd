using pandemic.Values;

namespace pandemic.Commands;

public record GovernmentGrantCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
