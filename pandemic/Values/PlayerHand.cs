namespace pandemic.Values;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameData;

public record PlayerHand
{
    public ImmutableArray<PlayerCard> Cards = [];

    private PlayerHand() { }

    public PlayerHand(IEnumerable<PlayerCard> cards)
    {
        Cards = [..cards];
    }

    public static readonly PlayerHand Empty = new();

    public static PlayerHand Of(params string[] cardNames)
    {
        return new PlayerHand(cardNames.Select(PlayerCards.CityCard));
    }

    public static PlayerHand Of(params PlayerCityCard[] cards)
    {
        return new PlayerHand(cards);
    }

    public static PlayerHand Of(IEnumerable<PlayerCityCard> cards)
    {
        return new PlayerHand(cards);
    }

    public static PlayerHand Of(params PlayerCard[] cards)
    {
        return new PlayerHand(cards);
    }

    public int Count => Cards.Length;

    public IEnumerable<PlayerCityCard> CityCards()
    {
        // perf:
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < Cards.Length; i++)
        {
            var card = Cards[i];
            if (card is PlayerCityCard cityCard)
            {
                yield return cityCard;
            }
        }
    }

    public PlayerHand Add(PlayerCard card)
    {
        return this with { Cards = Cards.Add(card) };
    }

    public PlayerHand Remove(PlayerCard card)
    {
        if (!Cards.Contains(card))
        {
            throw new InvalidOperationException($"{card} not in hand");
        }

        return this with
        {
            Cards = Cards.Remove(card),
        };
    }

    public bool Contains(PlayerCard card)
    {
        return Cards.Contains(card);
    }

    public IEnumerable<PlayerCard> SpecialEventCards()
    {
        for (int i = 0; i < Cards.Length; i++)
        {
            var card = Cards[i];
            if (card is ISpecialEventCard)
            {
                yield return card;
            }
        }
    }

    // Single is a very recognisable name for LINQ users
#pragma warning disable CA1720
    public PlayerCard Single(Func<PlayerCard, bool> func)
#pragma warning restore CA1720
    {
        PlayerCard? tempCard = null;

        for (int i = 0; i < Cards.Length; i++)
        {
            var card = Cards[i];
            if (func(card))
            {
                if (tempCard != null)
                {
                    throw new InvalidOperationException("More than one card matched the predicate");
                }

                tempCard = card;
            }
        }

        if (tempCard == null)
        {
            throw new InvalidOperationException("No cards matched the predicate");
        }

        return tempCard;
    }

    public PlayerCard First(Func<object, bool> func)
    {
        for (int i = 0; i < Cards.Length; i++)
        {
            var card = Cards[i];
            if (func(card))
            {
                return card;
            }
        }

        throw new InvalidOperationException("No cards matched the predicate");
    }

    public override string ToString()
    {
        return string.Join(", ", Cards);
    }
}
