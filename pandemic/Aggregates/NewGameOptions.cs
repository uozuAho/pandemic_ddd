using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Aggregates
{
    public record NewGameOptions
    {
        public Difficulty Difficulty { get; init; }
        public IEnumerable<Role> Roles { get; init; } = new List<Role>();
    }
}
