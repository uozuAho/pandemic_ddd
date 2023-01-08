using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Events;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.Aggregates.Game
{
    public partial record PandemicGame
    {
        public string LossReason { get; init; } = "";
        public Difficulty Difficulty { get; init; }
        public int InfectionRate => Board.InfectionRates[InfectionRateMarkerPosition];
        public int OutbreakCounter { get; init; }
        public int InfectionRateMarkerPosition { get; init; }
        public Player CurrentPlayer => Players[CurrentPlayerIdx];
        public int CurrentPlayerIdx { get; init; }
        public int ResearchStationPile { get; init; } = 5;
        public ImmutableList<Player> Players { get; init; } = ImmutableList<Player>.Empty;
        public ImmutableList<City> Cities { get; init; }
        public Deck<PlayerCard> PlayerDrawPile { get; init; } = Deck<PlayerCard>.Empty;
        public Deck<PlayerCard> PlayerDiscardPile { get; init; } = Deck<PlayerCard>.Empty;
        public Deck<InfectionCard> InfectionDrawPile { get; init; } = Deck<InfectionCard>.Empty;
        public Deck<InfectionCard> InfectionDiscardPile { get; init; } = Deck<InfectionCard>.Empty;
        public CubePile Cubes { get; init; } =
            new (ColourExtensions.AllColours.ToImmutableDictionary(c => c, _ => 24));
        public ImmutableDictionary<Colour, bool> CureDiscovered { get; init; } =
            ColourExtensions.AllColours.ToImmutableDictionary(c => c, _ => false);

        private Random Rng { get; } = new();

        public readonly StandardGameBoard Board = StandardGameBoard.Instance();

        public bool IsOver => IsLost || IsWon;

        public bool IsWon
        {
            get
            {
                foreach (var c in CureDiscovered)
                {
                    if (!c.Value) return false;
                }

                return true;
            }
        }
        public bool IsLost => LossReason != "";
        private bool APlayerMustDiscard => Players.Any(p => p.Hand.Count > 7);
        public TurnPhase PhaseOfTurn { get; init; } = TurnPhase.DoActions;

        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);

        public City CityByName(string city)
        {
            return Cities[Board.CityIdx(city)];
        }

        public bool IsSameStateAs(PandemicGame other)
        {
            if (LossReason != other.LossReason) return false;
            if (Difficulty != other.Difficulty) return false;
            if (InfectionRate != other.InfectionRate) return false;
            if (OutbreakCounter != other.OutbreakCounter) return false;
            if (CurrentPlayerIdx != other.CurrentPlayerIdx) return false;
            if (ResearchStationPile != other.ResearchStationPile) return false;

            // order is expected to be the same and significant (different order means not equal)
            if (!Players.SequenceEqual(other.Players, Player.DefaultEqualityComparer)) return false;
            if (!Cities.SequenceEqual(other.Cities, City.DefaultEqualityComparer)) return false;
            if (!InfectionDrawPile.IsSameAs(other.InfectionDrawPile)) return false;
            if (!InfectionDiscardPile.IsSameAs(other.InfectionDiscardPile)) return false;
            if (!PlayerDrawPile.IsSameAs(other.PlayerDrawPile)) return false;
            if (!Cubes.HasSameCubesAs(other.Cubes)) return false;
            if (!CureDiscovered.SequenceEqual(other.CureDiscovered)) return false;

            return true;
        }

        public static int NumberOfEpidemicCards(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Introductory => 4,
                Difficulty.Normal => 5,
                Difficulty.Heroic => 6,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
        }

        public static int InitialPlayerHandSize(int numberOfPlayers)
        {
            return numberOfPlayers switch
            {
                2 => 4,
                3 => 3,
                4 => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private PandemicGame(Random rng)
        {
            Cities = Board.Cities.Select(c => new City(c.Name)).ToImmutableList();

            var atlanta = CityByName("Atlanta");
            Cities = Cities.Replace(atlanta, atlanta with {HasResearchStation = true});

            PlayerDrawPile = new Deck<PlayerCard>(Board.Cities
                .Select(c => new PlayerCityCard(c) as PlayerCard));

            Rng = rng;
        }

        public static PandemicGame CreateUninitialisedGame(Random? rng = null) => new (rng ?? new Random());

        public static PandemicGame FromEvents(IEnumerable<IEvent> events)
        {
            // this code is easier to debug than events.Aggregate(CreateUninitialisedGame(), ApplyEvent);

            var game = CreateUninitialisedGame();
            var eventNumber = 0;

            foreach (var evt in events)
            {
                eventNumber++;
                game = ApplyEvent(game, evt);
            }

            return game;
        }

        public PandemicGame Copy()
        {
            return this with { };
        }

        public override string ToString()
        {
            return PandemicGameStringRenderer.FullState(this);
        }
    }
}
