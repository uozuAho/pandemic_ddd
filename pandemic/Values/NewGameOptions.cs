namespace pandemic.Values;

using System;
using System.Collections.Generic;
using Commands;

public record NewGameOptions
{
    public Difficulty Difficulty { get; init; } = Difficulty.Normal;
    public ICollection<Role> Roles { get; init; } = new List<Role>();
    public Random Rng { get; init; } = new();
    public bool IncludeSpecialEventCards { get; init; } = true;
    public ICommandGenerator CommandGenerator { get; init; } = new AllLegalCommandGenerator();

    public override string ToString()
    {
        return $"Difficulty: {Difficulty}, \n"
            + $"Roles: {string.Join(", ", Roles)}, \n"
            + $"IncludeSpecialEventCards: {IncludeSpecialEventCards}";
    }
}
