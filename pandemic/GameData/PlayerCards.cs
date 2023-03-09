using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public static class PlayerCards
    {
        public static IEnumerable<PlayerCityCard> CityCards => _allCards;

        public static PlayerCityCard CityCard(string name) => _cardLookup[name];

        private static readonly IEnumerable<PlayerCityCard> _allCards =
            StandardGameBoard.Instance().Cities.Select(c => new PlayerCityCard(c)).ToImmutableList();

        private static readonly Dictionary<string, PlayerCityCard> _cardLookup =
            _allCards.ToDictionary(c => c.City.Name, c => c);
    }
}
