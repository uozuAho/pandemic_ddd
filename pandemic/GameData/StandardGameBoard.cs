using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public class StandardGameBoard
    {
        public const int NumberOfCities = 48;

        public bool IsCity(string city)
        {
            return CityLookup.ContainsKey(city);
        }

        public bool IsAdjacent(string city1, string city2)
        {
            return AdjacentCities[city1].Contains(city2);
        }

        public CityData City(string name) => CityLookup[name];

        public IEnumerable<CityData> Cities => _cities;

        public int CityIdx(string name)
        {
            return _cityIdxLookup[name];
        }

        public readonly Dictionary<string, List<string>> AdjacentCities = CreateAdjacencyLookup();

        public int DriveFerryDistance(string city1, string city2)
        {
            // bfs
            var searched = new HashSet<string>();
            var queue = new Queue<(string city, int distance)>();
            queue.Enqueue((city1, 0));

            while (queue.Count > 0)
            {
                var (currentCity, distance) = queue.Dequeue();
                if (currentCity == city2) return distance;
                searched.Add(currentCity);
                foreach (var adj in AdjacentCities[currentCity])
                {
                    if (!searched.Contains(adj))
                        queue.Enqueue((adj, distance + 1));
                }
            }

            throw new InvalidOperationException("shouldn't get here");
        }

        private static Dictionary<string, List<string>> CreateAdjacencyLookup()
        {
            var lookup = _cities.ToDictionary(c => c.Name, _ => new List<string>());

            foreach (var (city1, city2) in Edges)
            {
                lookup[city1].Add(city2);
                lookup[city2].Add(city1);
            }

            return lookup;
        }

        private static readonly CityData[] _cities = {
            new("San Francisco", Colour.Blue),
            new("Chicago", Colour.Blue),
            new("Montreal", Colour.Blue),
            new("New York", Colour.Blue),
            new("Washington", Colour.Blue),
            new("Atlanta", Colour.Blue),
            new("Madrid", Colour.Blue),
            new("London", Colour.Blue),
            new("Paris", Colour.Blue),
            new("Essen", Colour.Blue),
            new("Milan", Colour.Blue),
            new("St. Petersburg", Colour.Blue),
            new("Algiers", Colour.Black),
            new("Istanbul", Colour.Black),
            new("Moscow", Colour.Black),
            new("Cairo", Colour.Black),
            new("Baghdad", Colour.Black),
            new("Tehran", Colour.Black),
            new("Delhi", Colour.Black),
            new("Karachi", Colour.Black),
            new("Riyadh", Colour.Black),
            new("Mumbai", Colour.Black),
            new("Chennai", Colour.Black),
            new("Kolkata", Colour.Black),
            new("Beijing", Colour.Red),
            new("Seoul", Colour.Red),
            new("Tokyo", Colour.Red),
            new("Shanghai", Colour.Red),
            new("Hong Kong", Colour.Red),
            new("Taipei", Colour.Red),
            new("Osaka", Colour.Red),
            new("Bangkok", Colour.Red),
            new("Ho Chi Minh City", Colour.Red),
            new("Manila", Colour.Red),
            new("Jakarta", Colour.Red),
            new("Sydney", Colour.Red),
            new("Khartoum", Colour.Yellow),
            new("Johannesburg", Colour.Yellow),
            new("Kinshasa", Colour.Yellow),
            new("Lagos", Colour.Yellow),
            new("Sao Paulo", Colour.Yellow),
            new("Buenos Aires", Colour.Yellow),
            new("Santiago", Colour.Yellow),
            new("Lima", Colour.Yellow),
            new("Bogota", Colour.Yellow),
            new("Mexico City", Colour.Yellow),
            new("Los Angeles", Colour.Yellow),
            new("Miami", Colour.Yellow)
        };

        private static readonly List<(string, string)> Edges = new()
        {
            ("San Francisco", "Chicago"),
            ("San Francisco", "Tokyo"),
            ("Chicago", "Montreal"),
            ("Chicago", "Atlanta"),
            ("New York", "Montreal"),
            ("New York", "Washington"),
            ("New York", "Madrid"),
            ("New York", "London"),
            ("Washington", "Montreal"),
            ("Washington", "Atlanta"),
            ("Washington", "Miami"),
            ("Madrid", "London"),
            ("Madrid", "Paris"),
            ("London", "Paris"),
            ("Essen", "London"),
            ("Essen", "Paris"),
            ("Essen", "Milan"),
            ("Essen", "St. Petersburg"),
            ("Milan", "Paris"),
            ("St. Petersburg", "Moscow"),
            ("Algiers", "Madrid"),
            ("Algiers", "Paris"),
            ("Algiers", "Istanbul"),
            ("Algiers", "Cairo"),
            ("Istanbul", "Milan"),
            ("Istanbul", "St. Petersburg"),
            ("Moscow", "Istanbul"),
            ("Cairo", "Istanbul"),
            ("Cairo", "Baghdad"),
            ("Cairo", "Khartoum"),
            ("Baghdad", "Istanbul"),
            ("Baghdad", "Tehran"),
            ("Tehran", "Moscow"),
            ("Tehran", "Delhi"),
            ("Tehran", "Karachi"),
            ("Delhi", "Chennai"),
            ("Delhi", "Kolkata"),
            ("Karachi", "Baghdad"),
            ("Karachi", "Delhi"),
            ("Karachi", "Mumbai"),
            ("Riyadh", "Cairo"),
            ("Riyadh", "Baghdad"),
            ("Riyadh", "Karachi"),
            ("Mumbai", "Delhi"),
            ("Mumbai", "Chennai"),
            ("Chennai", "Kolkata"),
            ("Chennai", "Bangkok"),
            ("Chennai", "Jakarta"),
            ("Kolkata", "Hong Kong"),
            ("Kolkata", "Bangkok"),
            ("Beijing", "Seoul"),
            ("Seoul", "Tokyo"),
            ("Tokyo", "Shanghai"),
            ("Shanghai", "Beijing"),
            ("Shanghai", "Seoul"),
            ("Shanghai", "Taipei"),
            ("Hong Kong", "Shanghai"),
            ("Hong Kong", "Taipei"),
            ("Taipei", "Osaka"),
            ("Osaka", "Tokyo"),
            ("Bangkok", "Hong Kong"),
            ("Bangkok", "Jakarta"),
            ("Ho Chi Minh City", "Hong Kong"),
            ("Ho Chi Minh City", "Bangkok"),
            ("Manila", "San Francisco"),
            ("Manila", "Hong Kong"),
            ("Manila", "Taipei"),
            ("Manila", "Ho Chi Minh City"),
            ("Jakarta", "Ho Chi Minh City"),
            ("Jakarta", "Sydney"),
            ("Sydney", "Manila"),
            ("Khartoum", "Johannesburg"),
            ("Johannesburg", "Kinshasa"),
            ("Kinshasa", "Khartoum"),
            ("Kinshasa", "Lagos"),
            ("Lagos", "Khartoum"),
            ("Sao Paulo", "Madrid"),
            ("Sao Paulo", "Lagos"),
            ("Sao Paulo", "Buenos Aires"),
            ("Sao Paulo", "Bogota"),
            ("Buenos Aires", "Bogota"),
            ("Lima", "Santiago"),
            ("Lima", "Mexico City"),
            ("Bogota", "Lima"),
            ("Mexico City", "Chicago"),
            ("Mexico City", "Bogota"),
            ("Mexico City", "Los Angeles"),
            ("Mexico City", "Miami"),
            ("Los Angeles", "San Francisco"),
            ("Los Angeles", "Chicago"),
            ("Los Angeles", "Sydney"),
            ("Miami", "Atlanta"),
            ("Miami", "Bogota"),
        };

        private static readonly Dictionary<string, CityData> CityLookup = _cities.ToDictionary(c => c.Name, c => c);

        private static readonly Dictionary<string, int> _cityIdxLookup = CreateCityIdxLookup();

        private static Dictionary<string, int> CreateCityIdxLookup()
        {
            var lookup = new Dictionary<string, int>();

            for (var i = 0; i < _cities.Length; i++)
            {
                lookup[_cities[i].Name] = i;
            }

            return lookup;
        }
    }
}
