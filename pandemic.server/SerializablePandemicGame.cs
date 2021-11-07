using System.Collections.Immutable;
using Newtonsoft.Json;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.server
{
    public record SerializablePandemicGame
    {
        public string LossReason { get; init; }
        public Difficulty Difficulty { get; init; }
        public int InfectionRate { get; init; }
        public int OutbreakCounter { get; init; }
        public int CurrentPlayerIdx { get; init; }
        public int ResearchStationPile { get; init; }
        public ImmutableList<Player> Players { get; init; }
        public ImmutableList<City> Cities { get; init; }
        public ImmutableList<PlayerCard> PlayerDrawPile { get; init; }
        public ImmutableList<PlayerCard> PlayerDiscardPile { get; init; }
        public ImmutableList<InfectionCard> InfectionDrawPile { get; init; }
        public ImmutableList<InfectionCard> InfectionDiscardPile { get; init; }
        public ImmutableDictionary<Colour, int> Cubes { get; init; }
        public ImmutableDictionary<Colour, bool> CureDiscovered { get; init; }

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
                PlayerDrawPile = game.PlayerDrawPile,
                PlayerDiscardPile = game.PlayerDiscardPile,
                InfectionDrawPile = game.InfectionDrawPile,
                InfectionDiscardPile = game.InfectionDiscardPile,
                Cubes = game.Cubes,
                CureDiscovered = game.CureDiscovered
            };
        }

        public PandemicGame ToPandemicGame()
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
                PlayerDrawPile = PlayerDrawPile,
                PlayerDiscardPile = PlayerDiscardPile,
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
            return JsonConvert.DeserializeObject<SerializablePandemicGame>(str);
        }
    }
}
