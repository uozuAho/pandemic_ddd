namespace pandemic.agents.GameEvaluator;

using Aggregates.Game;

public static class ResearchStations
{
    private static readonly int[] BestStationCityIdxs = [13, 5, 35, 19, 17];

    // 1 drive from the best
    private static readonly int[][] NextBestStations =
    [
        [3, 12, 20, 26, 41, 44],
        [6, 22, 27, 28, 39],
        [0, 11, 23, 25, 29],
        [16, 18, 21],
        [2, 10, 32, 36, 45],
    ];

    // 2 drives from the best
    private static readonly int[][] ThirdBestStations =
    [
        [4, 8, 10, 15, 34, 37, 40, 43, 46],
        [1, 9, 21, 24, 25, 38, 47],
        [7, 14, 33, 39, 42],
        [7, 39],
        [7, 8, 14, 20, 31],
    ];

    public static int Score(PandemicGame game)
    {
        var score = 0;

        // Searching from best outwards, find the first research station. Once
        // a station is found in the 'neighbourhood', no more stations contribute
        // to the score.
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

            if (found)
            {
                continue;
            }

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
}
