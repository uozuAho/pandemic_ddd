namespace pandemic.Commands;

using Values;

public record DispatcherCharterFlyPawnCommand(Role PlayerToMove, string Destination)
    : IPlayerCommand
{
    public Role Role => Role.Dispatcher;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
