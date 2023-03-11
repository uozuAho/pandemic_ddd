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
        public static int Evaluate(PandemicGame game)
        {
            if (game.IsWon) return int.MaxValue;
            if (game.IsLost) return int.MinValue;
            // todo: impossible to win = min value

            var score = 0;

            // diseases cured is great
            score += game.CuresDiscovered.Count * 1000;

            // research stations are good
            // even better: spread out (do later,,, maybe)
            score += game.Cities
                .Where(c => c.HasResearchStation)
                .Sum(_ => 100);

            score += game.Players
                .Select(p => p.Hand)
                .Sum(h => PlayerHandScore(game, h));

            // bad stuff -----------------------
            // outbreaks are bad
            score -= game.OutbreakCounter * 100;

            // todo: maybe use this when enough cards to cure
            // further away from research stations is bad
            // (at least with currently implemented rules)
            // score -= game.Players
            //     .Sum(p => StandardGameBoard.DriveFerryDistance(
            //         p.Location, ClosestResearchStationTo(game, p.Location)));

            // cubes
            for (int i = 0; i < game.Cities.Length; i++)
            {
                var city = game.Cities[i];
                foreach (var colour in ColourExtensions.AllColours)
                {
                    var cubes = city.Cubes.NumberOf(colour);
                    if (cubes == 0) continue;

                    // cubes in city
                    score -= cubes * cubes * 10;

                    // player distance from cubes
                    foreach (var player in game.Players)
                    {
                        var distance = StandardGameBoard.DriveFerryDistance(player.Location, city.Name);
                        score -= cubes * cubes * distance * 5;
                    }
                }
            }

            return score;
        }

        public static int PlayerHandScore(PandemicGame pandemicGame, PlayerHand hand)
        {
            // more cards of same colour = good, where colour is not cured
            //
            // each extra card of the same colour gains more points:
            // 1 blue = 0
            // 2 blue = 1 (0 + 1)
            // 3 blue = 3 (0 + 1 + 2)
            // 4 blue = 6 (0 + 1 + 2 + 3)
            // = n(n-1)/2

            var cured = pandemicGame.CuresDiscovered.Select(c => c.Colour);

            return hand.CityCards
                .GroupBy(c => c.City.Colour)
                .Where(g => !cured.Contains(g.Key))
                .Select(g => g.Count())
                .Sum(n => n * (n - 1) / 2);
        }

        // todo: perf: just do bfs. Worst case perf is same as this impl
        private static string ClosestResearchStationTo(PandemicGame game, string city)
        {
            var closest = "";
            var closestDistance = int.MaxValue;

            for (var i = 0; i < game.Cities.Length; i++)
            {
                var city1 = game.Cities[i];
                if (!city1.HasResearchStation) continue;

                var distance = StandardGameBoard.DriveFerryDistance(city1.Name, city);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = city1.Name;
                }
            }

            return closest;
        }
    }
}
