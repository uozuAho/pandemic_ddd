using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherMovePawnToOtherPawnCommand(Role PlayerToMove, Role DestinationRole) : IPlayerCommand
{
    public Role Role => Role.Dispatcher;

    public bool ConsumesAction => true;
}
