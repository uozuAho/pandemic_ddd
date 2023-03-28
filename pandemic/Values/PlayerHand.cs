using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.GameData;

namespace pandemic.Values
{
    public record PlayerHand
    {
        private ImmutableArray<PlayerCard> _cards = ImmutableArray<PlayerCard>.Empty;

        private PlayerHand()
        {
        }

        public PlayerHand(IEnumerable<PlayerCard> cards)
        {
            _cards = cards.ToImmutableArray();
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

        public IEnumerable<PlayerCard> Cards => _cards;

        public int Count => _cards.Length;

        public IEnumerable<PlayerCityCard> CityCards()
        {
            // perf:
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _cards.Length; i++)
            {
                var card = _cards[i];
                if (card is PlayerCityCard cityCard)
                    yield return cityCard;
            }
        }

        public PlayerHand Add(PlayerCard card)
        {
            return this with { _cards = _cards.Add(card) };
        }

        public PlayerHand Remove(PlayerCard card)
        {
            if (!_cards.Contains(card)) throw new InvalidOperationException($"{card} not in hand");

            return this with { _cards = _cards.Remove(card) };
        }

        public bool Contains(PlayerCard card)
        {
            return _cards.Contains(card);
        }

        public IEnumerable<PlayerCard> SpecialEventCards()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                var card = _cards[i];
                if (card is ISpecialEventCard specialEventCard)
                    yield return card;
            }
        }

        public PlayerCard Single(Func<PlayerCard, bool> func)
        {
            PlayerCard? tempCard = null;

            for (int i = 0; i < _cards.Length; i++)
            {
                var card = _cards[i];
                if (func(card))
                {
                    if (tempCard != null)
                        throw new InvalidOperationException("More than one card matched the predicate");
                    tempCard = card;
                }
            }

            if (tempCard == null)
                throw new InvalidOperationException("No cards matched the predicate");

            return tempCard;
        }

        public PlayerCard First(Func<object, bool> func)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                var card = _cards[i];
                if (func(card))
                    return card;
            }

            throw new InvalidOperationException("No cards matched the predicate");
        }

        public override string ToString()
        {
            return string.Join(", ", _cards);
        }
    }
}
