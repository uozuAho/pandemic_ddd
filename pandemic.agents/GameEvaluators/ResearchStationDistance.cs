namespace pandemic.agents.GameEvaluator;

using System;
using Aggregates.Game;
using GameData;

public static class ResearchStationDistance
{
    [ThreadStatic]
    private static int[] _distances;

    [ThreadStatic]
    private static int[] _queue;

    // CA1810: initialise static fields in place. Not applicable to ThreadStatic fields
#pragma warning disable CA1810
    static ResearchStationDistance()
#pragma warning restore CA1810
    {
        _distances = new int[48];
        _queue = new int[48];
    }

    // This currently just uses drive ferry distance, not shuttle, airlift etc.
    // Using primitive queue + city indexes for perf.
    public static (string, int) ClosestResearchStationTo(PandemicGame game, string city)
    {
        Array.Clear(_distances, 0, _distances.Length);
        Array.Clear(_queue, 0, _queue.Length);

        var queueHead = 0;
        var queueTail = 0;
        var startCityIdx = StandardGameBoard.CityIdx(city);
        _queue[queueTail++] = startCityIdx;
        _distances[startCityIdx] = 0;

        while (queueHead < StandardGameBoard.NumberOfCities)
        {
            var currentCityIdx = _queue[queueHead++];
            var currentCity = game.Cities[currentCityIdx];
            var distance = _distances[currentCityIdx];
            if (currentCity.HasResearchStation)
            {
                var cityName = currentCity.Name;
                return (cityName, distance);
            }

            _distances[currentCityIdx] = distance;
            var neighbours = StandardGameBoard.AdjacentCityIdxs(currentCityIdx);

            // ReSharper disable once ForCanBeConvertedToForeach
            // why? perf
            for (var i = 0; i < neighbours.Length; i++)
            {
                var neighbourIdx = neighbours[i];
                if (neighbourIdx == startCityIdx)
                {
                    continue;
                }

                if (_distances[neighbourIdx] != 0)
                {
                    continue;
                }

                _distances[neighbourIdx] = distance + 1;
                _queue[queueTail++] = neighbourIdx;
            }
        }

        throw new InvalidOperationException("shouldn't get here");
    }
}
