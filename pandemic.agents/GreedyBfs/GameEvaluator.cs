using System.Linq;
using pandemic.Aggregates;

namespace pandemic.agents.GreedyBfs
{
    public class GameEvaluator
    {
        /// <summary>
        /// Return a value that evaluates how 'good' a state is, ie.
        /// how likely a win is from this state. Higher values are
        /// better.
        /// </summary>
        public int Evaluate(PandemicGame game)
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

            // todo: cards of same colour in hand

            // bad stuff -----------------------
            // outbreaks are bad
            score -= game.OutbreakCounter * 100;

            return 0;
        }
    }
}
