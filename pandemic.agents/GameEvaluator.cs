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

            var score = 0;

            // diseases cured is great
            score += game.CuresDiscovered.Count * 100000;

            // research stations are good
            // even better: spread out (do later,,, maybe)
            score += game.Cities
                .Where(c => c.HasResearchStation)
                .Sum(_ => 100);

            // outbreaks are bad
            score -= game.OutbreakCounter * 100;

            score += CubesOnCitiesScore(game);
            score += game.Players.Select(p => PlayerScore(game, p)).Sum();

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
            else
            {
                score += DistanceFromCubesScore(game, player);
            }

            return score;
        }

        /// <summary>
        /// Higher score = players further away from cubes
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static int PlayerDistanceFromCubesScore(PandemicGame game)
        {
            var score = 0;

            for (int i = 0; i < game.Cities.Length; i++)
            {
                var city = game.Cities[i];
                foreach (var colour in ColourExtensions.AllColours)
                {
                    var cubes = city.Cubes.NumberOf(colour);
                    if (cubes == 0) continue;

                    foreach (var player in game.Players)
                    {
                        var distance = StandardGameBoard.DriveFerryDistance(player.Location, city.Name);
                        score -= cubes * cubes * distance * 5;
                    }
                }
            }

            return score;
        }

        // maybe: shouldn't be penalised for being far from one 3-cube city while being close to another
        /// <summary>
        /// Higher score = players closer to cubes
        /// </summary>
        private static int DistanceFromCubesScore(PandemicGame game, Player player)
        {
            var score = 0;

            for (int i = 0; i < game.Cities.Length; i++)
            {
                var city = game.Cities[i];
                foreach (var colour in ColourExtensions.AllColours)
                {
                    var cubes = city.Cubes.NumberOf(colour);
                    if (cubes == 0) continue;

                    var distance = StandardGameBoard.DriveFerryDistance(player.Location, city.Name);
                    score -= cubes * cubes * distance;
                }
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

                    score -= cubes * cubes * 10;
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

            // this currently just uses drive ferry distance, not shuttle, airlift etc.
            var searched = new HashSet<string>();
            var queue = new Queue<(string city, int distance)>();
            queue.Enqueue((city, 0));

            while (queue.Count > 0)
            {
                var (currentCity, distance) = queue.Dequeue();
                if (game.CityByName(currentCity).HasResearchStation) return (currentCity, distance);
                searched.Add(currentCity);
                foreach (var adj in game.Board.AdjacentCities[currentCity])
                {
                    if (!searched.Contains(adj))
                        queue.Enqueue((adj, distance + 1));
                }
            }

            throw new InvalidOperationException("shouldn't get here");
        }
    }
}
