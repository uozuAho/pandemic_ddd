using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents;

/// <summary>
/// Hand-crafted comparer that attempts to rank commands by likelihood
/// that they'll lead to a win. Uses multiple dispatch as found here:
/// https://stackoverflow.com/questions/480443/what-is-single-and-multiple-dispatch-in-relation-to-net/4810826#4810826
/// </summary>
public class CommandPriorityComparer : IComparer<IPlayerCommand>
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

    public int Compare(IPlayerCommand? a, IPlayerCommand? b)
    {
        if (a == null || b == null) throw new ArgumentException();

        return CompareMulti(a, b, 0);
    }

    // int parameter is just to distinguish from other overloads. This
    // allows multiple dispatch by casting the given commands as dynamic
    private int CompareMulti(IPlayerCommand a, IPlayerCommand b, int _)
    {
        var result = CompareMulti((dynamic)a, (dynamic)b);
        if (result != Unmatched) return result;
        result = CompareMulti((dynamic)b, (dynamic)a);
        if (result != Unmatched) return -result;

        // No explicit comparison has been defined for the given commands.
        // I'm tired of having to define these, so I'm just going to give
        // them the same priority instead of throwing an exception here.
        return Same;
    }

    /// <summary>
    /// This method is called when the commands cannot be resolved to more specific methods, ie.
    /// no comparison method has been defined for the two types.
    /// </summary>
    private int CompareMulti(IPlayerCommand a, IPlayerCommand b) => Unmatched;
    private int CompareMulti<T>(T a, T b) where T : IPlayerCommand => Same;
    private int CompareMulti(DiscoverCureCommand a, IPlayerCommand b) => Greater;
    private int CompareMulti(BuildResearchStationCommand a, DiscoverCureCommand b) => Less;
    private int CompareMulti(BuildResearchStationCommand a, IPlayerCommand b)
    {
        if (_game.HasResearchStationOnColour(StandardGameBoard.City(a.City).Colour))
            return Less;

        return Greater;
    }
    private int CompareMulti(DriveFerryCommand a, DiscardPlayerCardCommand b) => Greater;

    // I dunno when direct flight will be better/worse than any other command. Treat them
    // as same priority for now.
    private int CompareMulti(DirectFlightCommand a, IPlayerCommand b) => Same;

    private int CompareMulti(DirectFlightCommand a, DriveFerryCommand b)
    {
        return StandardGameBoard.IsAdjacent(a.Destination, _game.CurrentPlayer.Location) ? Less : Same;
    }

    private int CompareMulti(CharterFlightCommand a, IPlayerCommand b) => Same;

    private int CompareMulti(CharterFlightCommand a, DriveFerryCommand b)
    {
        return StandardGameBoard.IsAdjacent(a.Destination, _game.CurrentPlayer.Location) ? Less : Same;
    }

    private int CompareMulti(ShuttleFlightCommand a, IPlayerCommand b) => Same;

    private int CompareMulti(ShuttleFlightCommand a, CharterFlightCommand b) => Greater;
    private int CompareMulti(ShuttleFlightCommand a, DirectFlightCommand b) => Greater;
}

internal static class PandemicGameExtensions
{
    public static bool HasResearchStationOnColour(this PandemicGame game, Colour colour)
    {
        return StandardGameBoard.Cities.Where(c => c.Colour == colour)
            .Any(c => game.CityByName(c.Name).HasResearchStation);
    }
}
