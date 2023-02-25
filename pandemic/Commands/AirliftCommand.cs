using pandemic.Values;

namespace pandemic.Commands;

public record AirliftCommand(Role Role, string City) : IPlayerCommand;
