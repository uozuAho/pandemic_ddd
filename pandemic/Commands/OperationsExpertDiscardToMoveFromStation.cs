namespace pandemic.Commands;

using Values;

public record OperationsExpertDiscardToMoveFromStation(PlayerCityCard Card, string Destination)
    : IPlayerCommand
{
    public Role Role => Role.OperationsExpert;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
