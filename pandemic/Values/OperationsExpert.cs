namespace pandemic.Values;

public record OperationsExpert : Player
{
    public override Role Role => Role.OperationsExpert;
    public bool HasUsedDiscardAndMoveAbilityThisTurn { get; init; } = false;
}
