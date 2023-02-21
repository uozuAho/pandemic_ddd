using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

        public readonly StandardGameBoard Board = StandardGameBoard.Instance();

        public bool SelfConsistencyCheckingEnabled { get; init; } = true;

        private Random Rng { get; } = new();

        public ImmutableList<CureMarker> CuresDiscovered { get; init; } = ImmutableList<CureMarker>.Empty;

        public bool IsOver => IsLost || IsWon;
        public bool IsWon => CuresDiscovered.Count == 4;
        public bool IsLost => LossReason != "";
        public TurnPhase PhaseOfTurn { get; init; } = TurnPhase.DoActions;
        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);
        public City CityByName(string city) => Cities[Board.CityIdx(city)];

        public bool IsCured(Colour colour) =>
            CuresDiscovered.SingleOrDefault(c => c.Colour == colour) is not null;

        public bool IsEradicated(Colour colour) =>
            CuresDiscovered.SingleOrDefault(m => m.Colour == colour)?.ShowingSide == CureMarkerSide.Sunset;

        public bool APlayerMustDiscard => Players.Any(p => p.Hand.Count > 7);

        /// <summary>
        /// Players had the option to use a special event card, and chose not to
        /// </summary>
        private bool SkipNextChanceToUseSpecialEvent { get; init; }

        public bool APlayerHasASpecialEventCard => Players.Any(p => p.Hand.Any(c => c is ISpecialEventCard));

        /// <summary>
        /// Number of cards drawn during the current 'draw cards' phase
        /// </summary>
        private int CardsDrawn { get; init; }

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
            if (!CuresDiscovered.SequenceEqual(other.CuresDiscovered)) return false;

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

        private void ValidateInternalConsistency()
        {
            var totalCubes = Cubes.Counts().Values.Sum()
                             + Cities.Sum(c => c.Cubes.Counts().Sum(cc => cc.Value));
            Debug.Assert(totalCubes == 96);

            foreach (var numCubes in Cubes.Counts())
            {
                Debug.Assert(numCubes.Value is >= 0 and <= 24);
            }

            foreach (var numCubes in Cities.SelectMany(city => city.Cubes.Counts()))
            {
                Debug.Assert(numCubes.Value is >= 0 and <= 3);
            }

            var totalPlayerCards = Players.Select(p => p.Hand.Count).Sum()
                                   + PlayerDrawPile.Count
                                   + PlayerDiscardPile.Count;
            Debug.Assert(totalPlayerCards == 48 + NumberOfEpidemicCards(Difficulty) + SpecialEventCards.All.Count);

            Debug.Assert(InfectionDrawPile.Count + InfectionDiscardPile.Count == 48);

            Debug.Assert(ResearchStationPile + Cities.Count(c => c.HasResearchStation) == 6);
        }
    }
}
