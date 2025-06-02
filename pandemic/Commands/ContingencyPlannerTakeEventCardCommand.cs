namespace pandemic.Commands;

using Values;

public record ContingencyPlannerTakeEventCardCommand(ISpecialEventCard Card) : IPlayerCommand
{
    public Role Role => Role.ContingencyPlanner;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
