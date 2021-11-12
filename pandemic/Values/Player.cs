using System.Collections.Generic;
using System.Linq;

namespace pandemic.Values
{
    public record Player
    {
        public Role Role { get; init; }
        public string Location { get; init; } = "Atlanta";
        public PlayerHand Hand { get; init; } = PlayerHand.Empty;
        public int ActionsRemaining { get; init; } = 4;

        public bool IsSameStateAs(Player other)
        {
            if (Role != other.Role) return false;
            if (Location != other.Location) return false;
            if (!Hand.SequenceEqual(other.Hand, PlayerCard.DefaultEqualityComparer)) return false;
            if (ActionsRemaining != other.ActionsRemaining) return false;

            return true;
        }

        public static IEqualityComparer<Player> DefaultEqualityComparer = new PlayerComparer();
    }

    internal class PlayerComparer : IEqualityComparer<Player>
    {
        public bool Equals(Player? x, Player? y)
        {
            if (x == null || y == null) return false;

            return x.IsSameStateAs(y);
        }

        public int GetHashCode(Player obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
