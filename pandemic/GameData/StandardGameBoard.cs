using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public class StandardGameBoard
    {
        private StandardGameBoard() {}

        private static readonly CityData[] _cities = {
            new("Algiers", Colour.Black),
            new("Atlanta", Colour.Blue),
            new("Baghdad", Colour.Black),
            new("Bangkok", Colour.Red),
            new("Beijing", Colour.Red),
            new("Bogota", Colour.Yellow),
            new("Buenos Aires", Colour.Yellow),
            new("Cairo", Colour.Black),
            new("Chennai", Colour.Black),
            new("Chicago", Colour.Blue),
            new("Delhi", Colour.Black),
            new("Essen", Colour.Blue),
            new("Ho Chi Minh City", Colour.Red),
            new("Hong Kong", Colour.Red),
            new("Istanbul", Colour.Black),
            new("Jakarta", Colour.Red),
            new("Johannesburg", Colour.Yellow),
            new("Karachi", Colour.Black),
            new("Khartoum", Colour.Yellow),
            new("Kinshasa", Colour.Yellow),
            new("Kolkata", Colour.Black),
            new("Lagos", Colour.Yellow),
            new("Lima", Colour.Yellow),
            new("London", Colour.Blue),
            new("Los Angeles", Colour.Yellow),
            new("Madrid", Colour.Blue),
            new("Manila", Colour.Red),
            new("Mexico City", Colour.Yellow),
            new("Miami", Colour.Yellow),
            new("Milan", Colour.Blue),
            new("Montreal", Colour.Blue),
            new("Moscow", Colour.Black),
            new("Mumbai", Colour.Black),
            new("New York", Colour.Blue),
            new("Osaka", Colour.Red),
            new("Paris", Colour.Blue),
            new("Riyadh", Colour.Black),
            new("San Francisco", Colour.Blue),
            new("Santiago", Colour.Yellow),
            new("Sao Paulo", Colour.Yellow),
            new("Seoul", Colour.Red),
            new("Shanghai", Colour.Red),
            new("St. Petersburg", Colour.Blue),
            new("Sydney", Colour.Red),
            new("Taipei", Colour.Red),
            new("Tehran", Colour.Black),
            new("Tokyo", Colour.Red),
            new("Washington", Colour.Blue),
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

        public static StandardGameBoard Instance()
        {
            return _instance;
        }

        public const int NumberOfCities = 48;

        public readonly int[] InfectionRates = { 2, 2, 2, 3, 3, 4, 4 };

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

        private static Dictionary<string, List<string>> _adjacencyLookup = CreateAdjacencyLookup();

        public Dictionary<string, List<string>> AdjacentCities => _adjacencyLookup;

        private static readonly Dictionary<(string, string), int> _distanceLookup = BuildDriveFerryDistanceLookup();

        public static int DriveFerryDistance(string city1, string city2)
        {
            return _distanceLookup[(city1, city2)];
        }

        private static Dictionary<(string, string), int> BuildDriveFerryDistanceLookup()
        {
            var lookup = new Dictionary<(string, string), int>();

            foreach (var city1 in _cities)
            {
                foreach (var city2 in _cities)
                {
                    var distance = CalculateDriveFerryDistance(city1.Name, city2.Name);
                    lookup[(city1.Name, city2.Name)] = distance;
                }
            }

            return lookup;
        }

        private static int CalculateDriveFerryDistance(string city1, string city2)
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
                foreach (var adj in _adjacencyLookup[currentCity])
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

        private static readonly StandardGameBoard _instance = new();
    }
}
