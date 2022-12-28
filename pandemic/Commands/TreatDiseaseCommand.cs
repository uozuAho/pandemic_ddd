using pandemic.Values;

namespace pandemic.Commands;

public record TreatDiseaseCommand(Role Role, string City, Colour Colour) : IPlayerCommand;
