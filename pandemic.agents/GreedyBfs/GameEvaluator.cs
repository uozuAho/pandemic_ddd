using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.GreedyBfs
{
    public class GameEvaluator
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

            // further away from research stations is bad
            // (at least with currently implemented rules)
            score -= game.Players
                .Sum(p => StandardGameBoard.DriveFerryDistance(
                    p.Location, ClosestResearchStationTo(game, p.Location)));

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

        private static string ClosestResearchStationTo(PandemicGame game, string city)
        {
            var closest = "";
            var closestDistance = int.MaxValue;

            foreach (var researchCity in game.Cities
                         .Where(c => c.HasResearchStation)
                         .Select(c => c.Name))
            {
                var distance = StandardGameBoard.DriveFerryDistance(researchCity, city);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = researchCity;
                }
            }

            return closest;
        }
    }
}
