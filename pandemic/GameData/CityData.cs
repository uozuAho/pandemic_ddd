using System.Collections.Generic;
using pandemic.Values;

namespace pandemic.GameData
{
    public record CityData
    {
        public string Name { get; init; } = "";
        public Colour Colour { get; init; }
        // todo: get rid of this: move adjacency to `Board`
        //    why? cos printing adjacent cities is noisy
        public List<string> AdjacentCities { get; set; } = new();
    }
}
