using pandemic.Values;

namespace pandemic.Commands;

public record ContingencyPlannerSpecialEventCommand(IPlayerCommand Command) : IPlayerCommand
{
    public Role Role => Role.ContingencyPlanner;
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;
}
