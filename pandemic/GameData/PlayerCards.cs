namespace pandemic.GameData;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Values;

public static class PlayerCards
{
    public static IEnumerable<PlayerCityCard> CityCards { get; } =
        StandardGameBoard.Cities.Select(c => new PlayerCityCard(c)).ToImmutableList();

    public static PlayerCityCard CityCard(string name)
    {
        return _cardLookup[name];
    }

    private static readonly Dictionary<string, PlayerCityCard> _cardLookup = CityCards.ToDictionary(
        c => c.City.Name,
        c => c
    );
}
