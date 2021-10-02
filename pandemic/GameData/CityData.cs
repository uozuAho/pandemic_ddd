using pandemic.Values;

namespace pandemic.GameData
{
    public record CityData
    {
        public string Name { get; init; } = "";
        public Colour Colour { get; init; }
    }
}
