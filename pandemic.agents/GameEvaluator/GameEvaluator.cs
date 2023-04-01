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

        private static int ResearchStationScore(PandemicGame game)
        {
            var score = 0;

            // best chosen by me
            // rationale: spread them around highly-connected/remote cities
            var best = new[] { "Hong Kong", "Bogota", "Paris", "Kinshasa", "Karachi" };
            foreach (var city in best)
            {
                var (closestCity, distance) = ResearchStationDistance.ClosestResearchStationTo(game, city);
                score += 100 / (distance + 1);
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
            var cards = hand.Cards.ToArray();
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
