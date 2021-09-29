using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.test.Utils
{
    internal class GameBuilder
    {
        public static PandemicGame InitialiseNewGame()
        {
            var eventLog = new List<IEvent>();
            var game = PandemicGame.FromEvents(eventLog);

            game = game.SetDifficulty(eventLog, Difficulty.Normal);
            game = game.SetInfectionRate(eventLog, 2);
            game = game.SetOutbreakCounter(eventLog, 0);
            game = game.SetupInfectionDeck(eventLog);
            game = game.AddPlayer(eventLog, Role.Medic);

            return game;
        }
    }
}
