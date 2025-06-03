namespace pandemic.Values;

public record ContingencyPlanner : Player
{
    public override Role Role => Role.ContingencyPlanner;
    public ISpecialEventCard? StoredEventCard { get; init; }

    public override bool Has(PlayerCard card)
    {
        return base.Has(card) || (StoredEventCard != null && Equals(StoredEventCard, card));
    }
}
