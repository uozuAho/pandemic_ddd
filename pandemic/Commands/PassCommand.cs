namespace pandemic.Commands;

using Values;

public record PassCommand(Role Role) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
