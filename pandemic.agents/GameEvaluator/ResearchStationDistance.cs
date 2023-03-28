using System;
using pandemic.Aggregates.Game;

namespace pandemic.agents.GameEvaluator;

internal static class ResearchStationDistance
{
    [ThreadStatic] private static int[] _distances;
    [ThreadStatic] private static int[] _queue;

    static ResearchStationDistance()
    {
        _distances = new int[48];
        _queue = new int[48];
    }

    // This currently just uses drive ferry distance, not shuttle, airlift etc.
    // Using primitive queue + city indexes for perf.
    public static (string, int) ClosestResearchStationTo(PandemicGame game, string city)
    {
        if (game.CityByName(city).HasResearchStation) return (city, 0);

        Array.Clear(_distances, 0, _distances.Length);
        Array.Clear(_queue, 0, _queue.Length);

        var queueHead = 0;
        var queueTail = 0;
        var startCityIdx = game.Board.CityIdx(city);
        _queue[queueTail++] = game.Board.CityIdx(city);
        _distances[startCityIdx] = 0;

        while (queueHead < game.Cities.Length)
        {
            var currentCityIdx = _queue[queueHead++];
            var distance = _distances[currentCityIdx];
            var cityName = game.Cities[currentCityIdx].Name;
            if (game.Cities[currentCityIdx].HasResearchStation)
                return (cityName, distance);
            _distances[currentCityIdx] = distance;
            var neighbours = game.Board.AdjacentCities[cityName];

            // ReSharper disable once ForCanBeConvertedToForeach
            // why? perf
            for (var i = 0; i < neighbours.Count; i++)
            {
                var neighbourIdx = game.Board.CityIdx(neighbours[i]);
                if (neighbourIdx == startCityIdx) continue;
                if (_distances[neighbourIdx] != 0) continue;
                _distances[neighbourIdx] = distance + 1;
                _queue[queueTail++] = neighbourIdx;
            }
        }

        throw new InvalidOperationException("shouldn't get here");
    }
}
