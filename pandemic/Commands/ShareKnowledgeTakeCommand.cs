using pandemic.Values;

namespace pandemic.Commands;

public record ShareKnowledgeTakeCommand(Role Role, string City, Role TakeFromRole) : IPlayerCommand, IConsumesAction;
