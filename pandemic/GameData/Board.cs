using System.Collections.Generic;
using System.Linq;

namespace pandemic.GameData
{
    internal class Board
    {
        public Board()
        {
            foreach (var (city1, city2) in Edges)
            {
                CityLookup[city1].AdjacentCities.Add(city2);
                CityLookup[city2].AdjacentCities.Add(city1);
            }
        }

        public bool IsCity(string city)
        {
            return CityLookup.ContainsKey(city);
        }

        public bool IsAdjacent(string playerLocation, string city)
        {
            return CityLookup[playerLocation].AdjacentCities.Contains(city);
        }

        private static readonly City[] Cities = {
            new City
            {
                Name = "San Francisco",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Chicago",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Montreal",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "New York",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Washington",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Atlanta",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Madrid",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "London",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Paris",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Essen",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Milan",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "St. Petersburg",
                Colour = Colour.Blue,
            },
            new City
            {
                Name = "Algiers",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Istanbul",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Moscow",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Cairo",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Baghdad",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Tehran",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Delhi",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Karachi",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Riyadh",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Mumbai",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Chennai",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Kolkata",
                Colour = Colour.Black,
            },
            new City
            {
                Name = "Beijing",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Seoul",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Tokyo",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Shanghai",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Hong Kong",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Taipei",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Osaka",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Bangkok",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Ho Chi Minh City",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Manila",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Jakarta",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Sydney",
                Colour = Colour.Red,
            },
            new City
            {
                Name = "Khartoum",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Johannesburg",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Kinshasa",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Lagos",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Sao Paulo",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Buenos Aires",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Santiago",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Lima",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Bogota",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Mexico City",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Los Angeles",
                Colour = Colour.Yellow,
            },
            new City
            {
                Name = "Miami",
                Colour = Colour.Yellow,
            }
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

        private static readonly Dictionary<string, City> CityLookup = Cities.ToDictionary(c => c.Name, c => c);
    }
}
