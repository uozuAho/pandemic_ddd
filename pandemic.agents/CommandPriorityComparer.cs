using System;
using System.Collections.Generic;
using pandemic.Aggregates;

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

    private static int CompareMulti(PlayerCommand a, PlayerCommand b, int _)
    {
        var result = CompareMulti((dynamic)a, (dynamic)b);
        if (result != Unmatched) return result;
        result = CompareMulti((dynamic)b, (dynamic)a);
        if (result != Unmatched) return -result;

        throw new ArgumentException("Undefined comparison");
    }

    private static int CompareMulti(PlayerCommand a, PlayerCommand b) => Unmatched;
    private static int CompareMulti<T>(T a, T b) where T : PlayerCommand => Same;
    private static int CompareMulti(DiscoverCureCommand a, BuildResearchStationCommand b) => Greater;
    private static int CompareMulti(DiscoverCureCommand a, DriveFerryCommand b) => Greater;
    private static int CompareMulti(DiscoverCureCommand a, DiscardPlayerCardCommand b) => Greater;
    private static int CompareMulti(BuildResearchStationCommand a, DriveFerryCommand b) => Greater;
    private static int CompareMulti(BuildResearchStationCommand a, DiscardPlayerCardCommand b) => Greater;
    private static int CompareMulti(DriveFerryCommand a, DiscardPlayerCardCommand b) => Greater;
}
