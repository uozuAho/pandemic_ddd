using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherMovePawnToOtherPawnCommand(Role Role, Role DestinationRole) : IPlayerCommand, IConsumesAction;
