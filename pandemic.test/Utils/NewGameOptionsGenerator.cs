namespace pandemic.test.Utils;

using System;
using System.Linq;
using utils;
using Values;

public static class NewGameOptionsGenerator
{
    public static NewGameOptions RandomOptions()
    {
        var random = new Random();
        var numberOfPlayers = random.Choice(new[] { 2, 3, 4 });

        return new NewGameOptions
        {
            Difficulty = random.Choice(Enum.GetValues<Difficulty>()),
            Roles = random.Choice(numberOfPlayers, Enum.GetValues<Role>()).ToList(),
            IncludeSpecialEventCards = true,
        };
    }
}
