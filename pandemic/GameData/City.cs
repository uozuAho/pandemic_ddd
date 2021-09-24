namespace pandemic.GameData
{
    internal record City
    {
        public string Name { get; init; } = "";
        public Colour Colour { get; set; }
    }

    internal enum Colour
    {
        Blue,
        Black,
        Red,
        Yellow
    }
}
