using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherMovePawnToOtherPawnCommand(Role PlayerToMove, Role DestinationRole) : IPlayerCommand, IConsumesAction
{
    public Role Role => Role.Dispatcher;
}
