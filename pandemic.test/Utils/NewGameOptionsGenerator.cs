using System;
using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.test.Utils
{
    public static class NewGameOptionsGenerator
    {
        public static IEnumerable<NewGameOptions> AllOptions()
        {
            foreach (var difficulty in Enum.GetValues<Difficulty>())
            {
                yield return new NewGameOptions
                {
                    Difficulty = difficulty,
                    Roles = new[] {Role.Medic, Role.Scientist}
                };
            }

            // todo: more players
        }
    }
}
