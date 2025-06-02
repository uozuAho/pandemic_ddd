namespace pandemic.Commands;

using Values;

public record DispatcherMovePawnToOtherPawnCommand(Role PlayerToMove, Role DestinationRole)
    : IPlayerCommand
{
    public Role Role => Role.Dispatcher;

    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role}: move {PlayerToMove} to {DestinationRole}";
    }
}
