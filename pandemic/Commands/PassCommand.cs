using pandemic.Values;

namespace pandemic.Commands;

public record PassCommand(Role Role) : IPlayerCommand
{
    public bool ConsumesAction => true;
}
