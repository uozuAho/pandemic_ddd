using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public class PlayerCards
    {
        public static IEnumerable<PlayerCityCard> CityCards => _allCards;

        public static PlayerCityCard CityCard(string name) => _allCards.Single(c => c.City.Name == name);

        private static IEnumerable<PlayerCityCard> _allCards =>
            StandardGameBoard.Instance().Cities.Select(c => new PlayerCityCard(c)).ToList();
    }
}
