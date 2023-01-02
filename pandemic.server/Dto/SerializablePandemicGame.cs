using System;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.server.Dto
{
    public record SerializablePandemicGame
    {
        public string LossReason { get; init; } = "";
        public Difficulty Difficulty { get; init; }
        public int InfectionRateMarkerPosition { get; init; }
        public int OutbreakCounter { get; init; }
        public int CurrentPlayerIdx { get; init; }
        public int ResearchStationPile { get; init; }
        public ImmutableList<SerializablePlayer> Players { get; init; } = ImmutableList<SerializablePlayer>.Empty;
        public ImmutableList<SerializableCity> Cities { get; init; } = ImmutableList<SerializableCity>.Empty;
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
                InfectionRateMarkerPosition = game.InfectionRateMarkerPosition,
                OutbreakCounter = game.OutbreakCounter,
                CurrentPlayerIdx = game.CurrentPlayerIdx,
                ResearchStationPile = game.ResearchStationPile,
                Players = game.Players.Select(SerializablePlayer.From).ToImmutableList(),
                Cities = game.Cities.Select(SerializableCity.From).ToImmutableList(),
                PlayerDrawPile = game.PlayerDrawPile.Cards
                    .Select(SerializablePlayerCard.From).ToImmutableList(),
                PlayerDiscardPile = game.PlayerDiscardPile.Cards
                    .Select(SerializablePlayerCard.From).ToImmutableList(),
                InfectionDrawPile = game.InfectionDrawPile.Cards.ToImmutableList(),
                InfectionDiscardPile = game.InfectionDiscardPile.Cards.ToImmutableList(),
                Cubes = game.Cubes.Counts().ToImmutableDictionary(),
                CureDiscovered = game.CureDiscovered
            };
        }

        public PandemicGame ToPandemicGame(StandardGameBoard board)
        {
            return PandemicGame.CreateUninitialisedGame() with
            {
                LossReason = LossReason,
                Difficulty = Difficulty,
                InfectionRateMarkerPosition = InfectionRateMarkerPosition,
                OutbreakCounter = OutbreakCounter,
                CurrentPlayerIdx = CurrentPlayerIdx,
                ResearchStationPile = ResearchStationPile,
                Players = Players.Select(p => p.ToPlayer(board)).ToImmutableList(),
                Cities = Cities.Select(c => c.ToCity()).ToImmutableList(),
                PlayerDrawPile = new Deck<PlayerCard>(PlayerDrawPile
                    .Select(c => c.ToPlayerCard(board))),
                PlayerDiscardPile = new Deck<PlayerCard>(PlayerDiscardPile
                    .Select(c => c.ToPlayerCard(board))),
                InfectionDrawPile = new Deck<InfectionCard>(InfectionDrawPile),
                InfectionDiscardPile = new Deck<InfectionCard>(InfectionDiscardPile),
                Cubes = new CubePile(Cubes),
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
