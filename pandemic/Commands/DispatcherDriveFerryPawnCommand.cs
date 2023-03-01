using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherDriveFerryPawnCommand(Role Role, string City) : IPlayerCommand;
