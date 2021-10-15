using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public class PlayerCards
    {
        public static IEnumerable<PlayerCityCard> CityCards => _allCards;

        private static IEnumerable<PlayerCityCard> _allCards =>
            new StandardGameBoard().Cities.Select(c => new PlayerCityCard(c)).ToList();
    }
}
