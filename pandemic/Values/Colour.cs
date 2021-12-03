using System;

namespace pandemic.Values
{
    public enum Colour
    {
        Blue,
        Black,
        Red,
        Yellow
    }

    public static class ColourExtensions
    {
        public static readonly Colour[] AllColours = Enum.GetValues<Colour>();
    }
}
