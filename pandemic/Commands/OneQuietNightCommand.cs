using pandemic.Values;

namespace pandemic.Commands;

public record OneQuietNightCommand(Role Role) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
