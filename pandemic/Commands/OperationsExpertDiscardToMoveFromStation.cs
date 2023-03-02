using pandemic.Values;

namespace pandemic.Commands;

public record OperationsExpertDiscardToMoveFromStation(PlayerCityCard Card, string Destination) : IPlayerCommand
{
    public Role Role => Role.OperationsExpert;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
