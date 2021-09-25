using System.Collections.Generic;

namespace pandemic.GameData
{
    internal record City
    {
        public string Name { get; init; } = "";
        public Colour Colour { get; init; }
        public List<string> AdjacentCities { get; set; } = new();
    }
}