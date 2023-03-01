using pandemic.Values;

namespace pandemic.Commands;

public record OneQuietNightCommand(Role Role) : IPlayerCommand, ISpecialEventCommand
{
    public bool ConsumesAction => false;
}
