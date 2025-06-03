namespace pandemic.agents.GameEvaluators;

using System;
using GameEvaluator;
using Aggregates.Game;
using Values;

public static class HandCraftedGameEvaluator
{
    /// <summary>
    /// Return a value that evaluates how 'good' a state is, ie.
    /// how likely a win is from this state. Higher values are
    /// better.
    /// </summary>
    public static int Score(PandemicGame game)
    {
        if (game.IsWon)
        {
            return int.MaxValue;
        }

        if (game.IsLost)
        {
            return int.MinValue;
        }

        var cureScore = game.CuresDiscovered.Count * 100000;
        var stationScore = ResearchStations.Score(game);
        var outbreakScore = -game.OutbreakCounter * 100;
        var cubeScore = CubesOnCitiesScore(game);
        var cubeDistanceScore = CubeDistance.PlayerDistanceFromCubesScore(game);
        var playerScore = 0;
        for (var i = 0; i < game.Players.Length; i++)
        {
            playerScore += PlayerScore(game, game.Players[i]);
        }
        var discardScore = PenaliseDiscards(game);

        return cureScore
            + stationScore
            + outbreakScore
            + cubeScore
            + cubeDistanceScore
            + playerScore
            + discardScore;
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
                    case Colour.Red:
                        red++;
                        break;
                    case Colour.Blue:
                        blue++;
                        break;
                    case Colour.Yellow:
                        yellow++;
                        break;
                    case Colour.Black:
                        black++;
                        break;
                    // ReSharper disable once RedundantEmptySwitchSection: IDE0010
                    default:
                        break;
                }
            }
        }

        const int cannotCurePenalty = 1000000;

        if (!game.IsCured(Colour.Red))
        {
            if (red > 7)
            {
                score -= cannotCurePenalty;
            }

            score -= red * red * 10;
        }

        if (!game.IsCured(Colour.Blue))
        {
            if (blue > 7)
            {
                score -= cannotCurePenalty;
            }

            score -= blue * blue * 10;
        }

        if (!game.IsCured(Colour.Yellow))
        {
            if (yellow > 7)
            {
                score -= cannotCurePenalty;
            }

            score -= yellow * yellow * 10;
        }

        if (!game.IsCured(Colour.Black))
        {
            if (black > 7)
            {
                score -= cannotCurePenalty;
            }

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
            var (_, distance) = ResearchStationDistance.ClosestResearchStationTo(
                game,
                player.Location
            );
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
            if (card is not PlayerCityCard cityCard)
            {
                continue;
            }

            switch (cityCard.City.Colour)
            {
                case Colour.Black:
                    blackCount++;
                    break;
                case Colour.Blue:
                    blueCount++;
                    break;
                case Colour.Red:
                    redCount++;
                    break;
                case Colour.Yellow:
                    yellowCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hand));
            }
        }

        var redScore = redCount * redCount * (game.IsCured(Colour.Red) ? -10 : 10);
        var blueScore = blueCount * blueCount * (game.IsCured(Colour.Blue) ? -10 : 10);
        var yellowScore = yellowCount * yellowCount * (game.IsCured(Colour.Yellow) ? -10 : 10);
        var blackScore = blackCount * blackCount * (game.IsCured(Colour.Black) ? -10 : 10);

        return redScore + blueScore + yellowScore + blackScore;
    }
}
