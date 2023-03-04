using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.GameData;

namespace pandemic.Values
{
    public record PlayerHand : IEnumerable<PlayerCard>
    {
        private ImmutableList<PlayerCard> Cards { get; init; } = ImmutableList<PlayerCard>.Empty;

        private PlayerHand()
        {
        }

        public PlayerHand(IEnumerable<PlayerCard> cards)
        {
            Cards = cards.ToImmutableList();
        }

        public static readonly PlayerHand Empty = new ();

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

        public IEnumerator<PlayerCard> GetEnumerator() => Cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => Cards.Count;

        public IEnumerable<PlayerCityCard> CityCards =>
            Cards.Where(c => c is PlayerCityCard).Cast<PlayerCityCard>();

        public PlayerHand Add(PlayerCard card)
        {
            return this with { Cards = Cards.Add(card) };
        }

        public PlayerHand Remove(PlayerCard card)
        {
            if (!Cards.Contains(card)) throw new InvalidOperationException($"{card} not in hand");

            return this with { Cards = Cards.Remove(card) };
        }
    }
}
