using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public static class PlayerCards
    {
        public static IEnumerable<PlayerCityCard> CityCards => _allCards;

        public static PlayerCityCard CityCard(string name) => _allCards.Single(c => c.City.Name == name);

        private static readonly IEnumerable<PlayerCityCard> _allCards =
            StandardGameBoard.Instance().Cities.Select(c => new PlayerCityCard(c)).ToImmutableList();
    }
}
