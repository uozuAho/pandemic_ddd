using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.agents;

public class CommandPriorityComparer : IComparer<PlayerCommand>
{
    private const int Less = -1;
    private const int Same = 0;
    private const int Greater = 1;
    private const int Unmatched = 2;

    private readonly PandemicGame _game;

    public CommandPriorityComparer(PandemicGame game)
    {
        _game = game;
    }

    public int Compare(PlayerCommand? a, PlayerCommand? b)
    {
        if (a == null || b == null) throw new ArgumentException();

        return CompareMulti(a, b, 0);
    }

    private int CompareMulti(PlayerCommand a, PlayerCommand b, int _)
    {
        var result = CompareMulti((dynamic)a, (dynamic)b);
        if (result != Unmatched) return result;
        result = CompareMulti((dynamic)b, (dynamic)a);
        if (result != Unmatched) return -result;

        throw new ArgumentException("Undefined comparison");
    }

    private int CompareMulti(PlayerCommand a, PlayerCommand b) => Unmatched;
    private int CompareMulti<T>(T a, T b) where T : PlayerCommand => Same;
    private int CompareMulti(DiscoverCureCommand a, PlayerCommand b) => Greater;
    private int CompareMulti(BuildResearchStationCommand a, DiscoverCureCommand b) => Less;
    private int CompareMulti(BuildResearchStationCommand a, PlayerCommand b)
    {
        if (_game.HasResearchStationOnColour(_game.Board.City(a.City).Colour))
            return Less;

        return Greater;
    }
    private int CompareMulti(DriveFerryCommand a, DiscardPlayerCardCommand b) => Greater;
}

internal static class PandemicGameExtensions
{
    public static bool HasResearchStationOnColour(this PandemicGame game, Colour colour)
    {
        return game.Board.Cities.Where(c => c.Colour == colour)
            .Any(c => game.CityByName(c.Name).HasResearchStation);
    }
}
