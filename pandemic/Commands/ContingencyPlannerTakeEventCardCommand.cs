using pandemic.Values;

namespace pandemic.Commands;

public record ContingencyPlannerTakeEventCardCommand(ISpecialEventCard Card) : IPlayerCommand
{
    public Role Role => Role.ContingencyPlanner;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
