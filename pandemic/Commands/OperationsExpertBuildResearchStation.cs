namespace pandemic.Commands;

using Values;

public record OperationsExpertBuildResearchStation : IPlayerCommand
{
    public Role Role => Role.OperationsExpert;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
