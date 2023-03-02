using pandemic.Values;

namespace pandemic.Commands;

public record OperationsExpertBuildResearchStation : IPlayerCommand
{
    public Role Role => Role.OperationsExpert;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
