using System.Collections.Generic;
using pandemic.Events;

namespace pandemic
{
    public class Pandemic
    {
        public static PandemicGame NewGame()
        {
            return new PandemicGame();
        }

        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }
    }
}
