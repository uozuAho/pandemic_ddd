using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.Aggregates
{
    public record NewGameOptions(
        Difficulty Difficulty,
        IEnumerable<Role> Roles);
}
