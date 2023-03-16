namespace pandemic.Values;

public enum CureMarkerSide
{
    /// <summary>
    /// Vial = cured
    /// </summary>
    Vial,

    /// <summary>
    /// Sunset = eradicated
    /// </summary>
    Sunset
};

public record CureMarker(Colour Colour, CureMarkerSide ShowingSide)
{
    public CureMarker AsEradicated()
    {
        return this with { ShowingSide = CureMarkerSide.Sunset };
    }
}
