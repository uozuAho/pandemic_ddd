using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherDriveFerryPawnCommand(Role PlayerToMove, string City) : IPlayerCommand
{
    public Role Role => Role.Dispatcher;

    public bool ConsumesAction => true;
}
