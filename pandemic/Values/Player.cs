using System.Collections.Generic;

namespace pandemic.Values
{
    public record Player
    {
        public Role Role { get; init; }
        public string Location { get; init; } = "Atlanta";
        public List<PlayerCard> Hand { get; init; } = new();
        public int ActionsRemaining { get; init; } = 4;
    }
}
