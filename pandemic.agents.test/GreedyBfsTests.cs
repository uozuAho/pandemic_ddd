using NUnit.Framework;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class GreedyBfsTests
    {
        [Test]
        public void Does_step()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist },
            });

            var problem = new PandemicSearchProblem(game);
            var evaluator = new GameEvaluator();

            var bfs = new GreedyBestFirstSearch<PandemicGame, PlayerCommand>(problem, GameEvaluator.Evaluate);

            bfs.Step();
        }
    }
}
