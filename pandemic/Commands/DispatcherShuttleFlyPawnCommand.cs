using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherShuttleFlyPawnCommand(Role PlayerToMove, string City) : IPlayerCommand
{
    public Role Role => Role.Dispatcher;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
