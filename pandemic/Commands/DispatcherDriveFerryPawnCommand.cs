using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherDriveFerryPawnCommand(Role PlayerToMove, string City) : IPlayerCommand, IConsumesAction
{
    public Role Role => Role.Dispatcher;
}
