namespace pandemic.Values;

public record ContingencyPlanner : Player
{
    public override Role Role => Role.ContingencyPlanner;
    public ISpecialEventCard? StoredEventCard { get; init; } = null;
}
