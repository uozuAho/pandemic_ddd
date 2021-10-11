using pandemic.Values;

namespace pandemic.GameData
{
    public record CityData
    {
        // todo: use property shortcut?
        public string Name { get; init; } = "";
        public Colour Colour { get; init; }
    }
}
