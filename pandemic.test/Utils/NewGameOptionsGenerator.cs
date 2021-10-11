using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.test.Utils
{
    public static class NewGameOptionsGenerator
    {
        public static IEnumerable<NewGameOptions> AllOptions()
        {
            foreach (var difficulty in Enum.GetValues<Difficulty>())
            {
                foreach (var numPlayers in new[] {2, 3, 4})
                {
                    yield return new NewGameOptions
                    {
                        Difficulty = difficulty,
                        Roles = Enum.GetValues<Role>().Take(numPlayers).ToArray()
                    };
                }
                
            }
        }
    }
}
