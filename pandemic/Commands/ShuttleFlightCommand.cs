using pandemic.Values;

namespace pandemic.Commands;

public record ShuttleFlightCommand(Role Role, string City) : PlayerCommand;
