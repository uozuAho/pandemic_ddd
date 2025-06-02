namespace pandemic.Commands;

using Values;

public record OneQuietNightCommand(Role Role) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
