namespace pandemic.Commands;

using Values;

public record DispatcherShuttleFlyPawnCommand(Role PlayerToMove, string City) : IPlayerCommand
{
    public Role Role => Role.Dispatcher;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
