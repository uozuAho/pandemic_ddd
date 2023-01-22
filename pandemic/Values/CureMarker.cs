namespace pandemic.Values;

internal enum CureMarkerSide
{
    Vial,
    Sunset
};

internal record CureMarker(Colour Colour, CureMarkerSide ShowingSide);
