namespace pandemic.Values;

using System;

public enum Colour
{
    Blue,
    Black,
    Red,
    Yellow,
}

public static class ColourExtensions
{
    public static readonly Colour[] AllColours = Enum.GetValues<Colour>();
}
