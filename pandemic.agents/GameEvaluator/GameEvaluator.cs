using System;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.GameEvaluator
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
            var cubeDistanceScore = CubeDistance.PlayerDistanceFromCubesScore(game);
            var playerScore = 0;
            for (var i = 0; i < game.Players.Length; i++)
            {
                playerScore += PlayerScore(game, game.Players[i]);
            }
            var discardScore = PenaliseDiscards(game);

            return cureScore + stationScore + outbreakScore + cubeScore + cubeDistanceScore + playerScore + discardScore;
        }

        private static readonly int[] BestStationCityIdxs = { 13, 5, 35, 19, 17 };

        // 1 drive from the best
        private static readonly int[][] NextBestStations =
        {
            new[] { 3, 12, 20, 26, 41, 44 },
            new[] { 6, 22, 27, 28, 39 },
            new[] { 0, 11, 23, 25, 29 },
            new[] { 16, 18, 21 },
            new[] { 2, 10, 32, 36, 45 },
        };

        // 2 drives from the best
        private static readonly int[][] ThirdBestStations =
        {
            new[] { 4, 8, 10, 15, 34, 37, 40, 43, 46 },
            new[] { 1, 9, 21, 24, 25, 38, 47 },
            new[] { 7, 14, 33, 39, 42 },
            new[] { 7, 39 },
            new[] { 7, 8, 14, 20, 31 },
        };

        private static int ResearchStationScore(PandemicGame game)
        {
            var score = 0;

            for (var i = 0; i < 5; i++)
            {
                var best = BestStationCityIdxs[i];

                if (game.Cities[best].HasResearchStation)
                {
                    score += 100;
                    continue;
                }

                var found = false;
                var nextBests = NextBestStations[i];
                for (var j = 0; j < nextBests.Length; j++)
                {
                    if (game.Cities[nextBests[j]].HasResearchStation)
                    {
                        score += 50;
                        found = true;
                        break;
                    }
                }

                if (found) continue;

                var nextNextBests = ThirdBestStations[i];
                for (var j = 0; j < nextNextBests.Length; j++)
                {
                    if (game.Cities[nextNextBests[j]].HasResearchStation)
                    {
                        score += 25;
                        break;
                    }
                }
            }

            return score;
        }

        private static int PenaliseDiscards(PandemicGame game)
        {
            var score = 0;

            var red = 0;
            var blue = 0;
            var yellow = 0;
            var black = 0;

            foreach (var card in game.PlayerDiscardPile.Cards)
            {
                if (card is PlayerCityCard cityCard)
                {
                    switch (cityCard.City.Colour)
                    {
                        case Colour.Red: red++; break;
                        case Colour.Blue: blue++; break;
                        case Colour.Yellow: yellow++; break;
                        case Colour.Black: black++; break;
                    }
                }
            }

            const int cannotCurePenalty = 1000000;

            if (!game.IsCured(Colour.Red))
            {
                if (red > 7) score -= cannotCurePenalty;
                score -= red * red * 10;
            }

            if (!game.IsCured(Colour.Blue))
            {
                if (blue > 7) score -= cannotCurePenalty;
                score -= blue * blue * 10;
            }

            if (!game.IsCured(Colour.Yellow))
            {
                if (yellow > 7) score -= cannotCurePenalty;
                score -= yellow * yellow * 10;
            }

            if (!game.IsCured(Colour.Black))
            {
                if (black > 7) score -= cannotCurePenalty;
                score -= black * black * 10;
            }

            return score;
        }

        private static int PlayerScore(PandemicGame game, Player player)
        {
            var score = 0;

            score += PlayerHandScore(game, player.Hand);

            if (player.HasEnoughToCure())
            {
                var (city, distance) = ResearchStationDistance.ClosestResearchStationTo(game, player.Location);
                score -= distance * 5;
            }

            return score;
        }

        /// <summary>
        /// Higher score = fewer cubes on cities
        /// </summary>
        private static int CubesOnCitiesScore(PandemicGame game)
        {
            var score = 0;

            for (var i = 0; i < game.Cities.Length; i++)
            {
                var city = game.Cities[i];
                var red = city.Cubes.Red;
                var blue = city.Cubes.Blue;
                var yellow = city.Cubes.Yellow;
                var black = city.Cubes.Black;

                score -= red * red * red * 10;
                score -= blue * blue * blue * 10;
                score -= yellow * yellow * yellow * 10;
                score -= black * black * black * 10;
            }

            return score;
        }

        public static int PlayerHandScore(PandemicGame game, PlayerHand hand)
        {
            var cards = hand.Cards;
            var redCount = 0;
            var blueCount = 0;
            var yellowCount = 0;
            var blackCount = 0;

            // perf:
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                if (card is not PlayerCityCard cityCard) continue;

                switch (cityCard.City.Colour)
                {
                    case Colour.Black: blackCount++; break;
                    case Colour.Blue: blueCount++; break;
                    case Colour.Red: redCount++; break;
                    case Colour.Yellow: yellowCount++; break;
                }
            }

            var redScore = redCount * redCount * (game.IsCured(Colour.Red) ? -10 : 10);
            var blueScore = blueCount * blueCount * (game.IsCured(Colour.Blue) ? -10 : 10);
            var yellowScore = yellowCount * yellowCount * (game.IsCured(Colour.Yellow) ? -10 : 10);
            var blackScore = blackCount * blackCount * (game.IsCured(Colour.Black) ? -10 : 10);

            return redScore + blueScore + yellowScore + blackScore;
        }
    }
}
