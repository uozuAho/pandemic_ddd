using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents
{
    public static class GameEvaluator
    {
        /// <summary>
        /// Return a value that evaluates how 'good' a state is, ie.
        /// how likely a win is from this state. Higher values are
        /// better.
        /// </summary>
        public static int Score(PandemicGame game)
        {
            if (game.IsWon) return int.MaxValue;
            if (game.IsLost) return int.MinValue;

            var cureScore = game.CuresDiscovered.Count * 100000;
            var stationScore = ResearchStationScore(game);
            var outbreakScore = -game.OutbreakCounter * 100;
            var cubeScore = CubesOnCitiesScore(game);
            var cubeDistanceScore = PlayerDistanceFromCubesScore(game);
            var playerScore = game.Players.Select(p => PlayerScore(game, p)).Sum();
            var discardScore = PenaliseDiscards(game);

            return cureScore + stationScore + outbreakScore + cubeScore + cubeDistanceScore + playerScore + discardScore;
        }

        private static int ResearchStationScore(PandemicGame game)
        {
            var score = 0;

            // best chosen by me
            // rationale: spread them around highly-connected/remote cities
            var best = new[] { "Hong Kong", "Bogota", "Paris", "Kinshasa", "Karachi" };
            foreach (var city in best)
            {
                var (closestCity, distance) = ClosestResearchStationTo(game, city);
                score += 100 / (distance + 1);
            }

            return score;
        }

        private static int PenaliseDiscards(PandemicGame game)
        {
            var score = 0;

            foreach (var cardsOfColour in game.PlayerDiscardPile.Cards
                         .Where(c => c is PlayerCityCard)
                         .Cast<PlayerCityCard>()
                         .GroupBy(c => c.City.Colour))
            {
                var colour = cardsOfColour.Key;
                var numCards = cardsOfColour.Count();
                if (!game.IsCured(colour))
                {
                    if (numCards > 7) score -= 1000000; // cannot cure!
                    score -= numCards * numCards * 10;
                }
            }

            return score;
        }

        private static int PlayerScore(PandemicGame game, Player player)
        {
            var score = 0;

            score += PlayerHandScore(game, player.Hand);

            if (player.HasEnoughToCure())
            {
                var (city, distance) = ClosestResearchStationTo(game, player.Location);
                score -= distance * 5;
            }

            return score;
        }

        private static int PlayerDistanceFromCubesScore(PandemicGame game)
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
            var players = game.Players.ToList();
            var cities = game.Cities;

            var cities3 = new List<City>();
            var cities2 = new List<City>();
            var cities1 = new List<City>();

            for (int i = 0; i < cities.Length; i++)
            {
                var city = cities[i];
                switch (city.MaxNumCubes())
                {
                    case 0: continue;
                    case 1: cities1.Add(city); break;
                    case 2: cities2.Add(city); break;
                    case 3: cities3.Add(city); break;
                }
            }

            var citiesToBlah = new Queue<City>(cities3.Concat(cities2).Concat(cities1));
            var score = 0;

            while (citiesToBlah.Count > 0 && players.Count > 0)
            {
                var city = citiesToBlah.Dequeue();
                var closestPlayer = players
                    .Where(p => !p.HasEnoughToCure())
                    .MinBy(p => StandardGameBoard.DriveFerryDistance(p.Location, city.Name));

                if (closestPlayer == null) break;

                var numCubes = city.MaxNumCubes();
                var distance = StandardGameBoard.DriveFerryDistance(closestPlayer.Location, city.Name);

                players.Remove(closestPlayer);

                score -= numCubes * numCubes * distance;
            }

            return score;
        }

        /// <summary>
        /// Higher score = fewer cubes on cities
        /// </summary>
        private static int CubesOnCitiesScore(PandemicGame game)
        {
            var score = 0;

            for (int i = 0; i < game.Cities.Length; i++)
            {
                var city = game.Cities[i];
                foreach (var colour in ColourExtensions.AllColours)
                {
                    var cubes = city.Cubes.NumberOf(colour);
                    if (cubes == 0) continue;

                    score -= cubes * cubes * cubes * 10;
                }
            }

            return score;
        }

        public static int PlayerHandScore(PandemicGame game, PlayerHand hand)
        {
            // more cards of same colour = good, where colour is not cured
            // each extra card of the same colour gains more points

            return + hand.CityCards
                       .GroupBy(c => c.City.Colour)
                       .Where(g => !game.IsCured(g.Key))
                       .Select(g => g.Count())
                       .Sum(n => n * n * 10)

                   - hand.CityCards
                       .GroupBy(c => c.City.Colour)
                       .Where(g => game.IsCured(g.Key))
                       .Select(g => g.Count())
                       .Sum(n => n * n * 10);
        }

        private static (string, int) ClosestResearchStationTo(PandemicGame game, string city)
        {
            if (game.CityByName(city).HasResearchStation) return (city, 0);

            // This currently just uses drive ferry distance, not shuttle, airlift etc.
            // Using primitive queue + city indexes for perf.
            var distances = new int[game.Cities.Length];
            var queue = new int[game.Cities.Length];
            var queueHead = 0;
            var queueTail = 0;
            var startCityIdx = game.Board.CityIdx(city);
            queue[queueTail++] = game.Board.CityIdx(city);
            distances[startCityIdx] = 0;

            while (queueHead < game.Cities.Length)
            {
                var currentCityIdx = queue[queueHead++];
                var distance = distances[currentCityIdx];
                var cityName = game.Cities[currentCityIdx].Name;
                if (game.Cities[currentCityIdx].HasResearchStation)
                    return (cityName, distance);
                distances[currentCityIdx] = distance;
                var neighbours = game.Board.AdjacentCities[cityName];

                // ReSharper disable once ForCanBeConvertedToForeach
                // why? perf
                for (var i = 0; i < neighbours.Count; i++)
                {
                    var neighbourIdx = game.Board.CityIdx(neighbours[i]);
                    if (neighbourIdx == startCityIdx) continue;
                    if (distances[neighbourIdx] != 0) continue;
                    distances[neighbourIdx] = distance + 1;
                    queue[queueTail++] = neighbourIdx;
                }
            }

            throw new InvalidOperationException("shouldn't get here");
        }
    }
}
