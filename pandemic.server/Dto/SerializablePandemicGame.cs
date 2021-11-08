using System;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using pandemic.Aggregates;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.server.Dto
{
    public record SerializablePandemicGame
    {
        public string LossReason { get; init; } = "";
        public Difficulty Difficulty { get; init; }
        public int InfectionRate { get; init; }
        public int OutbreakCounter { get; init; }
        public int CurrentPlayerIdx { get; init; }
        public int ResearchStationPile { get; init; }
        public ImmutableList<Player> Players { get; init; } = ImmutableList<Player>.Empty;
        public ImmutableList<City> Cities { get; init; } = ImmutableList<City>.Empty;
        public ImmutableList<SerializablePlayerCard> PlayerDrawPile { get; init; } = ImmutableList<SerializablePlayerCard>.Empty;
        public ImmutableList<SerializablePlayerCard> PlayerDiscardPile { get; init; } = ImmutableList<SerializablePlayerCard>.Empty;
        public ImmutableList<InfectionCard> InfectionDrawPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public ImmutableList<InfectionCard> InfectionDiscardPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public ImmutableDictionary<Colour, int> Cubes { get; init; } = ImmutableDictionary<Colour, int>.Empty;
        public ImmutableDictionary<Colour, bool> CureDiscovered { get; init; } = ImmutableDictionary<Colour, bool>.Empty;

        public static SerializablePandemicGame From(PandemicGame game)
        {
            return new SerializablePandemicGame
            {
                LossReason = game.LossReason,
                Difficulty = game.Difficulty,
                InfectionRate = game.InfectionRate,
                OutbreakCounter = game.OutbreakCounter,
                CurrentPlayerIdx = game.CurrentPlayerIdx,
                ResearchStationPile = game.ResearchStationPile,
                Players = game.Players,
                Cities = game.Cities,
                PlayerDrawPile = game.PlayerDrawPile
                    .Select(SerializablePlayerCard.From).ToImmutableList(),
                PlayerDiscardPile = game.PlayerDiscardPile
                    .Select(SerializablePlayerCard.From).ToImmutableList(),
                InfectionDrawPile = game.InfectionDrawPile,
                InfectionDiscardPile = game.InfectionDiscardPile,
                Cubes = game.Cubes,
                CureDiscovered = game.CureDiscovered
            };
        }

        public PandemicGame ToPandemicGame(StandardGameBoard board)
        {
            return PandemicGame.CreateUninitialisedGame() with
            {
                LossReason = LossReason,
                Difficulty = Difficulty,
                InfectionRate = InfectionRate,
                OutbreakCounter = OutbreakCounter,
                CurrentPlayerIdx = CurrentPlayerIdx,
                ResearchStationPile = ResearchStationPile,
                Players = Players,
                Cities = Cities,
                PlayerDrawPile = PlayerDrawPile
                    .Select(c => c.ToPlayerCard(board)).ToImmutableList(),
                PlayerDiscardPile = PlayerDiscardPile
                    .Select(c => c.ToPlayerCard(board)).ToImmutableList(),
                InfectionDrawPile = InfectionDrawPile,
                InfectionDiscardPile = InfectionDiscardPile,
                Cubes = Cubes,
                CureDiscovered = CureDiscovered
            };
        }

        public string Serialise()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SerializablePandemicGame Deserialise(string str)
        {
            var obj = JsonConvert.DeserializeObject<SerializablePandemicGame>(str);

            if (obj == null)
                throw new InvalidOperationException("Error deserializing game");

            return obj;
        }
    }
}
