using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Values;
using utils;

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
                        Roles = Enum.GetValues<Role>().Take(numPlayers).ToArray(),
                        IncludeSpecialEventCards = true
                    };
                }

            }
        }

        public static NewGameOptions RandomOptions()
        {
            var random = new Random();
            var numberOfPlayers = random.Choice(new[] { 2, 3, 4 });

            return new NewGameOptions
            {
                Difficulty = random.Choice(Enum.GetValues<Difficulty>()),
                Roles = random.Choice(numberOfPlayers, Enum.GetValues<Role>()).ToList(),
                IncludeSpecialEventCards = true
            };
        }
    }
}
