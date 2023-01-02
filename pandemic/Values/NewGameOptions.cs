using System;
using System.Collections.Generic;

namespace pandemic.Values
{
    public record NewGameOptions
    {
        public Difficulty Difficulty { get; init; } = Difficulty.Normal;
        public ICollection<Role> Roles { get; init; } = new List<Role>();
        public Random Rng { get; init; } = new();
    }
}
