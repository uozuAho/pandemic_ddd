using System.Collections.Generic;

namespace pandemic.Values
{
    public record NewGameOptions
    {
        public Difficulty Difficulty { get; init; }
        public ICollection<Role> Roles { get; init; } = new List<Role>();
    }
}
