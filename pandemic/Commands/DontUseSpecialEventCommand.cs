using pandemic.Values;

namespace pandemic.Commands;

public record DontUseSpecialEventCommand(Role Role) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => false;
}

public interface ICommand { }
