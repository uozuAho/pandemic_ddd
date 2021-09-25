using System.Collections.Immutable;

namespace pandemic.Values
{
    public record Player
    {
        public Role Role { get; init; }
        public string Location { get; init; } = "Atlanta";
        public ImmutableList<PlayerCard> Hand { get; init; } = ImmutableList<PlayerCard>.Empty;
        public int ActionsRemaining { get; init; } = 4;
    }
}
