using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.GameData
{
    public static class StandardGameBoard
    {
        public const int NumberOfCities = 48;
        public static readonly int[] InfectionRates = { 2, 2, 2, 3, 3, 4, 4 };
        public static readonly int NumberOfSpecialEventCards = SpecialEventCards.All.Count;
        public static bool IsCity(string city) => CityLookup.ContainsKey(city);
        public static CityData City(string name) => CityLookup[name];
        public static IEnumerable<CityData> Cities => _cities;
        public static int CityIdx(string name) => CityIdxLookup[name];
        public static readonly Dictionary<string, List<string>> AdjacentCities;
        public static int[] AdjacentCityIdxs(int cityIdx) => AdjacencyIdxsLookup[cityIdx];
        public static bool IsAdjacent(string city1, string city2) => AdjacentCities[city1].Contains(city2);
        public static int DriveFerryDistance(string city1, string city2) => DistanceLookup[(city1, city2)];

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
            ("Algiers", "Cairo"),
            ("Algiers", "Istanbul"),
            ("Algiers", "Madrid"),
            ("Algiers", "Paris"),
            ("Baghdad", "Istanbul"),
            ("Baghdad", "Tehran"),
            ("Bangkok", "Hong Kong"),
            ("Bangkok", "Jakarta"),
            ("Beijing", "Seoul"),
            ("Bogota", "Lima"),
            ("Buenos Aires", "Bogota"),
            ("Cairo", "Baghdad"),
            ("Cairo", "Istanbul"),
            ("Cairo", "Khartoum"),
            ("Chennai", "Bangkok"),
            ("Chennai", "Jakarta"),
            ("Chennai", "Kolkata"),
            ("Chicago", "Atlanta"),
            ("Chicago", "Montreal"),
            ("Delhi", "Chennai"),
            ("Delhi", "Kolkata"),
            ("Essen", "London"),
            ("Essen", "Milan"),
            ("Essen", "Paris"),
            ("Essen", "St. Petersburg"),
            ("Ho Chi Minh City", "Bangkok"),
            ("Ho Chi Minh City", "Hong Kong"),
            ("Hong Kong", "Shanghai"),
            ("Hong Kong", "Taipei"),
            ("Istanbul", "Milan"),
            ("Istanbul", "St. Petersburg"),
            ("Jakarta", "Ho Chi Minh City"),
            ("Jakarta", "Sydney"),
            ("Johannesburg", "Kinshasa"),
            ("Karachi", "Baghdad"),
            ("Karachi", "Delhi"),
            ("Karachi", "Mumbai"),
            ("Khartoum", "Johannesburg"),
            ("Kinshasa", "Khartoum"),
            ("Kinshasa", "Lagos"),
            ("Kolkata", "Bangkok"),
            ("Kolkata", "Hong Kong"),
            ("Lagos", "Khartoum"),
            ("Lima", "Mexico City"),
            ("Lima", "Santiago"),
            ("London", "Paris"),
            ("Los Angeles", "Chicago"),
            ("Los Angeles", "San Francisco"),
            ("Los Angeles", "Sydney"),
            ("Madrid", "London"),
            ("Madrid", "Paris"),
            ("Manila", "Ho Chi Minh City"),
            ("Manila", "Hong Kong"),
            ("Manila", "San Francisco"),
            ("Manila", "Taipei"),
            ("Mexico City", "Bogota"),
            ("Mexico City", "Chicago"),
            ("Mexico City", "Los Angeles"),
            ("Mexico City", "Miami"),
            ("Miami", "Atlanta"),
            ("Miami", "Bogota"),
            ("Milan", "Paris"),
            ("Moscow", "Istanbul"),
            ("Mumbai", "Chennai"),
            ("Mumbai", "Delhi"),
            ("New York", "London"),
            ("New York", "Madrid"),
            ("New York", "Montreal"),
            ("New York", "Washington"),
            ("Osaka", "Tokyo"),
            ("Riyadh", "Baghdad"),
            ("Riyadh", "Cairo"),
            ("Riyadh", "Karachi"),
            ("San Francisco", "Chicago"),
            ("San Francisco", "Tokyo"),
            ("Sao Paulo", "Bogota"),
            ("Sao Paulo", "Buenos Aires"),
            ("Sao Paulo", "Lagos"),
            ("Sao Paulo", "Madrid"),
            ("Seoul", "Tokyo"),
            ("Shanghai", "Beijing"),
            ("Shanghai", "Seoul"),
            ("Shanghai", "Taipei"),
            ("St. Petersburg", "Moscow"),
            ("Sydney", "Manila"),
            ("Taipei", "Osaka"),
            ("Tehran", "Delhi"),
            ("Tehran", "Karachi"),
            ("Tehran", "Moscow"),
            ("Tokyo", "Shanghai"),
            ("Washington", "Atlanta"),
            ("Washington", "Miami"),
            ("Washington", "Montreal"),
        };

        static StandardGameBoard()
        {
            CityLookup = _cities.ToDictionary(c => c.Name, c => c);
            CityIdxLookup = CreateCityIdxLookup();
            AdjacentCities = CreateAdjacencyLookup();
            AdjacencyIdxsLookup = CreateAdjacentIdxLookup();
            DistanceLookup = BuildDriveFerryDistanceLookup();
        }

        private static readonly Dictionary<string, CityData> CityLookup;
        private static readonly Dictionary<string, int> CityIdxLookup;
        private static readonly int[][] AdjacencyIdxsLookup;
        private static readonly Dictionary<(string, string), int> DistanceLookup;

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

        private static int[][] CreateAdjacentIdxLookup()
        {
            var lookup = new int[NumberOfCities][];

            for (var i = 0; i < NumberOfCities; i++)
            {
                var city = _cities[i];
                lookup[i] = AdjacentCities[city.Name].Select(CityIdx).ToArray();
            }

            return lookup;
        }

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
