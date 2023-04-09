using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.GameEvaluator;

internal static class CubeDistance
{
    [ThreadStatic] private static readonly City[] CityQueue;
    [ThreadStatic] private static int _queueHead;
    [ThreadStatic] private static int _queueTail;
    [ThreadStatic] private static readonly City[] Cities3;
    [ThreadStatic] private static readonly City[] Cities2;
    [ThreadStatic] private static readonly City[] Cities1;
    [ThreadStatic] private static readonly List<Player> Players;

    static CubeDistance()
    {
        CityQueue = new City[48];
        Cities3 = new City[48];
        Cities2 = new City[48];
        Cities1 = new City[48];
        Players = new List<Player>(4);
    }

    public static int PlayerDistanceFromCubesScore(PandemicGame game)
    {
        /*
             * pseudocode:
             *
             * for each city in order of max number of cubes:
             *    while there's still players that haven't been assigned a city:
             *        find the closest player that cannot cure
             *        assign that player to that city
             *        score the distance to that player
             */
        Players.Clear();
        for (var i = 0; i < game.Players.Length; i++)
        {
            var player = game.Players[i];
            if (!player.HasEnoughToCure())
                Players.Add(player);
        }
        SetCitiesDescendingByMaxCubes(game);
        var score = 0;

        while (_queueHead < _queueTail && Players.Count > 0)
        {
            var city = CityQueue[_queueHead++];
            var closestPlayer = Players
                .MinBy(p => StandardGameBoard.DriveFerryDistance(p.Location, city.Name));

            if (closestPlayer == null) break;

            var numCubes = city.MaxNumCubes();
            var distance = StandardGameBoard.DriveFerryDistance(closestPlayer.Location, city.Name);

            Players.Remove(closestPlayer);

            score -= numCubes * numCubes * distance;
        }

        return score;
    }

    private static void SetCitiesDescendingByMaxCubes(PandemicGame game)
    {
        var cities = game.Cities;
        var city3Idx = 0;
        var city2Idx = 0;
        var city1Idx = 0;

        for (var i = 0; i < cities.Length; i++)
        {
            var city = cities[i];
            switch (city.MaxNumCubes())
            {
                case 0: continue;
                case 1:
                    Cities1[city1Idx++] = city;
                    break;
                case 2:
                    Cities2[city2Idx++] = city;
                    break;
                case 3:
                    Cities3[city3Idx++] = city;
                    break;
            }
        }

        _queueHead = 0;
        _queueTail = 0;
        Array.Copy(Cities3, 0, CityQueue, _queueTail, city3Idx);
        _queueTail += city3Idx;
        Array.Copy(Cities2, 0, CityQueue, _queueTail, city2Idx);
        _queueTail += city2Idx;
        Array.Copy(Cities1, 0, CityQueue, _queueTail, city1Idx);
        _queueTail += city1Idx;
    }
}
