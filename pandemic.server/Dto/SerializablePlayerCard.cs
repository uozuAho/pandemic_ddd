using System;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.server.Dto
{
    public record SerializablePlayerCard
    {
        public string Type { get; init; } = "Error! Set me";
        public string? City { get; init; }

        public static SerializablePlayerCard From(PlayerCard card) => card switch
        {
            EpidemicCard e => From(e),
            PlayerCityCard p => From(p),
            _ => throw new InvalidOperationException($"Unknown card type {card.GetType()}")
        };

        public static SerializablePlayerCard From(EpidemicCard card) => new()
        {
            Type = "Epidemic"
        };

        public static SerializablePlayerCard From(PlayerCityCard card) => new()
        {
            Type = "City",
            City = card.City.Name
        };

        public PlayerCard ToPlayerCard(StandardGameBoard board)
        {
            return Type switch
            {
                "Epidemic" => new EpidemicCard(),
                "City" => ToPlayerCityCard(board),
                _ => throw new InvalidOperationException($"Unknown type {Type}")
            };
        }

        private PlayerCityCard ToPlayerCityCard(StandardGameBoard board)
        {
            return new PlayerCityCard(board.City(City ?? throw new InvalidOperationException("Null city!")));
        }
    }
}
