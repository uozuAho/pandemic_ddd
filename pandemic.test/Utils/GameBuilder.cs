using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.test.Utils
{
    internal class GameBuilder
    {
        public static PandemicGame InitialiseNewGame()
        {
            var game = PandemicGame.CreateUninitialisedGame();

            (game, _) = game.SetDifficulty(Difficulty.Normal);
            (game, _) = game.SetInfectionRate(2);
            (game, _) = game.SetOutbreakCounter(0);
            (game, _) = game.SetupInfectionDeck();
            (game, _) = game.AddPlayer(Role.Medic);

            return game;
        }
    }
}
