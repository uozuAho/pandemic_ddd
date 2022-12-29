using System.Collections.Immutable;
using pandemic.Values;

namespace pandemic.server.Dto;

public record SerializableCity(string Name)
{
    public IImmutableDictionary<Colour, int> Cubes = ImmutableDictionary<Colour, int>.Empty;
    public bool HasResearchStation;

    public static SerializableCity From(City city)
    {
        return new SerializableCity(city.Name)
        {
            Cubes = city.Cubes.Counts(),
            HasResearchStation = city.HasResearchStation
        };
    }

    public City ToCity()
    {
        return new City(Name)
        {
            HasResearchStation = HasResearchStation,
            Cubes = new CubePile(Cubes),
        };
    }
}
