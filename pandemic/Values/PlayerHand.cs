using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public static PlayerHand Empty = new ();

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
            return this with { Cards = Cards.Remove(card) };
        }
    }
}
