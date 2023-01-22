using System;

namespace pandemic.Values;

public enum CureMarkerSide
{
    Vial,
    Sunset
};

public record CureMarker(Colour Colour, CureMarkerSide ShowingSide)
{
    public CureMarker Flip()
    {
        var newShowingSide = ShowingSide switch
        {
            CureMarkerSide.Vial => CureMarkerSide.Sunset,
            CureMarkerSide.Sunset => CureMarkerSide.Vial,
            _ => throw new ArgumentOutOfRangeException()
        };

        return this with { ShowingSide = newShowingSide };
    }
}
