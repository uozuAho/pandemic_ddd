﻿using System;
using System.Collections.Immutable;

namespace pandemic.Values;

public record CubePile
{
    public readonly int Red = 0;
    public readonly int Yellow = 0;
    public readonly int Black = 0;
    public readonly int Blue = 0;

    public int Total() => Red + Blue + Black + Yellow;

    private CubePile()
    {
    }

    public CubePile(IImmutableDictionary<Colour, int> cubes)
    {
        Red = cubes[Colour.Red];
        Blue = cubes[Colour.Blue];
        Black = cubes[Colour.Black];
        Yellow = cubes[Colour.Yellow];
    }

    public static readonly CubePile Empty = new();

    private CubePile(int red, int blue, int black, int yellow)
    {
        Red = red;
        Blue = blue;
        Black = black;
        Yellow = yellow;
    }

    public bool HasSameCubesAs(CubePile other)
    {
        return Red == other.Red && Blue == other.Blue && Black == other.Black && Yellow == other.Yellow;
    }

    public CubePile AddCube(Colour colour)
    {
        return AddCubes(colour, 1);
    }

    public CubePile AddCubes(Colour colour, int numCubes)
    {
        return colour switch
        {
            Colour.Red => new CubePile(Red + numCubes, Blue, Black, Yellow),
            Colour.Blue => new CubePile(Red, Blue + numCubes, Black, Yellow),
            Colour.Black => new CubePile(Red, Blue, Black + numCubes, Yellow),
            Colour.Yellow => new CubePile(Red, Blue, Black, Yellow + numCubes),
            _ => throw new ArgumentOutOfRangeException(nameof(colour), colour, null)
        };
    }

    public CubePile RemoveCubes(Colour colour, int numCubes)
    {
        return colour switch
        {
            Colour.Red => new CubePile(Red - numCubes, Blue, Black, Yellow),
            Colour.Blue => new CubePile(Red, Blue - numCubes, Black, Yellow),
            Colour.Black => new CubePile(Red, Blue, Black - numCubes, Yellow),
            Colour.Yellow => new CubePile(Red, Blue, Black, Yellow - numCubes),
            _ => throw new ArgumentOutOfRangeException(nameof(colour), colour, null)
        };
    }

    public CubePile RemoveCube(Colour colour)
    {
        return RemoveCubes(colour, 1);
    }

    public CubePile RemoveAll(Colour colour)
    {
        return colour switch
        {
            Colour.Red => new CubePile(0, Blue, Black, Yellow),
            Colour.Blue => new CubePile(Red, 0, Black, Yellow),
            Colour.Black => new CubePile(Red, Blue, 0, Yellow),
            Colour.Yellow => new CubePile(Red, Blue, Black, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(colour), colour, null)
        };
    }

    public bool Any()
    {
        return Red > 0 || Blue > 0 || Black > 0 || Yellow > 0;
    }

    public int NumberOf(Colour colour)
    {
        return colour switch
        {
            Colour.Red => Red,
            Colour.Blue => Blue,
            Colour.Black => Black,
            Colour.Yellow => Yellow,
            _ => throw new ArgumentOutOfRangeException(nameof(colour), colour, null)
        };
    }

    public override string ToString()
    {
        return "Red: " + Red + ", Blue: " + Blue + ", Black: " + Black + ", Yellow: " + Yellow;
    }
}
