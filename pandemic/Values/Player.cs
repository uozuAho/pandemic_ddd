namespace pandemic.Values;

using System.Collections.Generic;
using System.Linq;

public record Player
{
    public virtual Role Role { get; init; }
    public string Location { get; init; } = "Atlanta";
    public PlayerHand Hand { get; init; } = PlayerHand.Empty;
    public int ActionsRemaining { get; init; } = 4;

    public bool IsSameStateAs(Player other)
    {
        if (Role != other.Role)
        {
            return false;
        }

        if (Location != other.Location)
        {
            return false;
        }

        if (!Hand.Cards.SequenceEqual(other.Hand.Cards, PlayerCard.DefaultEqualityComparer))
        {
            return false;
        }

        if (ActionsRemaining != other.ActionsRemaining)
        {
            return false;
        }

        return true;
    }

    public static readonly IEqualityComparer<Player> DefaultEqualityComparer = new PlayerComparer();

    public virtual bool Has(PlayerCard card)
    {
        return Hand.Contains(card);
    }

    public bool HasEnoughToCure()
    {
        var neededToCure = Role == Role.Scientist ? 4 : 5;
        var black = 0;
        var blue = 0;
        var red = 0;
        var yellow = 0;

        foreach (var card in Hand.Cards)
        {
            if (card is PlayerCityCard cityCard)
            {
                switch (cityCard.City.Colour)
                {
                    case Colour.Black:
                        black++;
                        break;
                    case Colour.Blue:
                        blue++;
                        break;
                    case Colour.Red:
                        red++;
                        break;
                    case Colour.Yellow:
                        yellow++;
                        break;
                    default:
                        break;
                }
            }
        }

        return black >= neededToCure
            || blue >= neededToCure
            || red >= neededToCure
            || yellow >= neededToCure;
    }
}

internal class PlayerComparer : IEqualityComparer<Player>
{
    public bool Equals(Player? x, Player? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.IsSameStateAs(y);
    }

    public int GetHashCode(Player obj)
    {
        throw new System.NotImplementedException();
    }
}
