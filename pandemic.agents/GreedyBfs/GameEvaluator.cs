using System.Linq;
using pandemic.Aggregates;
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
            score += game.CureDiscovered.Sum(c => c.Value ? 1000 : 0);

            // each research station on a unique color is good
            score += game.Cities
                .Where(c => c.HasResearchStation)
                .GroupBy(c => game.Board.City(c.Name).Colour)
                .Sum(_ => 100);

            score += game.Players.Select(p => p.Hand).Sum(PlayerHandScore);

            // bad stuff -----------------------
            // outbreaks are bad
            score -= game.OutbreakCounter * 100;

            return score;
        }

        public static int PlayerHandScore(PlayerHand hand)
        {
            // more cards of same colour = good
            //
            // each extra card of the same colour gains more points:
            // 1 blue = 0
            // 2 blue = 1 (0 + 1)
            // 3 blue = 3 (0 + 1 + 2)
            // 4 blue = 6 (0 + 1 + 2 + 3)
            // = n(n-1)/2
            return hand.CityCards
                .GroupBy(c => c.City.Colour)
                .Select(g => g.Count())
                .Sum(n => n * (n - 1) / 2);
        }
    }
}
