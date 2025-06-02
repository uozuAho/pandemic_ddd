namespace pandemic.Commands;

using Values;

public record ResearcherShareKnowledgeGiveCommand(Role PlayerToGiveTo, string City) : IPlayerCommand
{
    public Role Role => Role.Researcher;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
