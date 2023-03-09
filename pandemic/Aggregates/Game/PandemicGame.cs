using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using pandemic.Commands;
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
        public ImmutableArray<City> Cities { get; init; }
        public Deck<PlayerCard> PlayerDrawPile { get; init; } = Deck<PlayerCard>.Empty;
        public Deck<PlayerCard> PlayerDiscardPile { get; init; } = Deck<PlayerCard>.Empty;
        public Deck<InfectionCard> InfectionDrawPile { get; init; } = Deck<InfectionCard>.Empty;
        public Deck<InfectionCard> InfectionDiscardPile { get; init; } = Deck<InfectionCard>.Empty;
        public CubePile Cubes { get; init; } =
            new (ColourExtensions.AllColours.ToImmutableDictionary(c => c, _ => 24));

        public readonly StandardGameBoard Board = StandardGameBoard.Instance();
        private readonly ICommandGenerator _commandGenerator;

        public IEnumerable<IPlayerCommand> LegalCommands()
        {
            return _commandGenerator.Commands(this);
        }

        public bool SelfConsistencyCheckingEnabled { get; init; } = true;

        private Random Rng { get; } = new();

        public ImmutableList<CureMarker> CuresDiscovered { get; init; } = ImmutableList<CureMarker>.Empty;

        public bool IsOver => IsLost || IsWon;
        public bool IsWon => CuresDiscovered.Count == 4;
        public bool IsLost => LossReason != "";
        public TurnPhase PhaseOfTurn { get; init; } = TurnPhase.DoActions;

        public Player PlayerByRole(Role role)
        {
            for (var i = 0; i < Players.Count; i++)
            {
                if (Players[i].Role == role) return Players[i];
            }

            throw new ArgumentException("No player with that role");
        }
        public City CityByName(string city) => Cities[Board.CityIdx(city)];

        public bool IsCured(Colour colour) =>
            CuresDiscovered.SingleOrDefault(c => c.Colour == colour) is not null;

        public bool IsEradicated(Colour colour) =>
            CuresDiscovered.SingleOrDefault(m => m.Colour == colour)?.ShowingSide == CureMarkerSide.Sunset;

        public bool APlayerMustDiscard => Players.Any(p => p.Hand.Count > 7);

        /// <summary>
        /// A special event was 'recently' chosen not to be used. Toggles back on when the
        /// next opportunity to use a special event arises.
        /// </summary>
        public bool SpecialEventWasRecentlySkipped { get; init; } = false;

        /// <summary>
        /// Number of cards drawn during the current 'draw cards' phase
        /// </summary>
        public int CardsDrawn { get; init; }

        private ImmutableList<InfectionCard> InfectionCardsRemovedFromGame { get; init; } = ImmutableList<InfectionCard>.Empty;

        private ImmutableList<PlayerCard> PlayerCardsRemovedFromGame { get; init; } = ImmutableList<PlayerCard>.Empty;

        /// <summary>
        /// enabled by using the 'one quiet night' special event card
        /// </summary>
        private bool OneQuietNightWillBeUsedNextInfectPhase { get; init; }

        private bool PlayerCommandRequired()
        {
            return !IsOver && _commandGenerator.Commands(this).Any();
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

        private PandemicGame(Random rng, ICommandGenerator commandGenerator)
        {
            Cities = Board.Cities.Select(c => new City(c.Name)).ToImmutableArray();

            var atlanta = CityByName("Atlanta");
            Cities = Cities.Replace(atlanta, atlanta with {HasResearchStation = true});

            PlayerDrawPile = new Deck<PlayerCard>(Board.Cities
                .Select(c => new PlayerCityCard(c) as PlayerCard));

            Rng = rng;
            _commandGenerator = commandGenerator;
        }

        public static PandemicGame CreateUninitialisedGame(Random? rng = null, ICommandGenerator? commandGenerator = null)
        {
            return new(rng ?? new Random(), commandGenerator ?? new AllLegalCommandGenerator());
        }

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

        public PandemicGame Cure(Colour colour)
        {
            if (CuresDiscovered.Any(c => c.Colour == colour))
                throw new InvalidOperationException($"{colour} is already cured");

            return this with
            {
                CuresDiscovered = CuresDiscovered.Add(new CureMarker(colour, CureMarkerSide.Vial))
            };
        }

        public PandemicGame AddCube(string city, Colour colour)
        {
            return AddCubes(city, colour, 1);
        }

        public PandemicGame AddCubes(string city, Colour colour, int numCubes)
        {
            var city_ = CityByName(city);
            var cityIdx = Board.CityIdx(city);

            return this with
            {
                Cities = Cities.SetItem(cityIdx, city_ with { Cubes = city_.Cubes.AddCubes(colour, numCubes) })
            };
        }

        private bool DoesQuarantineSpecialistPreventInfectionAt(string city)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                if (player.Role == Role.QuarantineSpecialist)
                {
                    if (player.Location == city || QuarantineSpecialistIsInNeighbouringCity(city))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsMedicAt(string city)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                if (player.Role == Role.Medic && player.Location == city)
                {
                    return true;
                }
            }

            return false;
        }

        private bool QuarantineSpecialistIsInNeighbouringCity(string city)
        {
            var neighbouringCities = Board.AdjacentCities[city];

            return neighbouringCities.Any(c => PlayerByRole(Role.QuarantineSpecialist).Location == c);
        }

        public override string ToString()
        {
            return PandemicGameStringRenderer.FullState(this);
        }

        private void ValidateInternalConsistency()
        {
            Debug.Assert(TotalCubesInGame() == 96);

            foreach (var numCubes in Cubes.Counts)
            {
                Debug.Assert(numCubes.Value is >= 0 and <= 24);
            }

            foreach (var city in Cities)
            {
                Debug.Assert(city.Cubes.NumberOf(Colour.Black) is >= 0 and <= 3);
                Debug.Assert(city.Cubes.NumberOf(Colour.Blue) is >= 0 and <= 3);
                Debug.Assert(city.Cubes.NumberOf(Colour.Red) is >= 0 and <= 3);
                Debug.Assert(city.Cubes.NumberOf(Colour.Yellow) is >= 0 and <= 3);
            }

            var totalPlayerCards = Players.Select(p => p.Hand.Count).Sum()
                                   + PlayerDrawPile.Count
                                   + PlayerDiscardPile.Count
                                   + PlayerCardsRemovedFromGame.Count;
            if (ContingencyPlannerStoredCard != null) totalPlayerCards++;
            Debug.Assert(totalPlayerCards == 48 + NumberOfEpidemicCards(Difficulty) + SpecialEventCards.All.Count);

            var specialEventCards = Players
                .SelectMany(p => p.Hand.Cards)
                .Concat(PlayerDrawPile.Cards)
                .Concat(PlayerDiscardPile.Cards)
                .Concat(PlayerCardsRemovedFromGame)
                .Concat(new[] { ContingencyPlannerStoredCard as PlayerCard })
                .Where(c => c is ISpecialEventCard)
                .ToList();

            Debug.Assert(specialEventCards.Count == SpecialEventCards.All.Count);
            Debug.Assert(specialEventCards.ToHashSet().Count == SpecialEventCards.All.Count);

            Debug.Assert(
                InfectionDrawPile.Count
                + InfectionDiscardPile.Count
                + InfectionCardsRemovedFromGame.Count == 48);

            Debug.Assert(ResearchStationPile + Cities.Count(c => c.HasResearchStation) == 6);

            if (Players.Any(p => p.Role == Role.Medic))
            {
                foreach (var curedColour in ColourExtensions.AllColours.Where(IsCured))
                {
                    var medicLocation = PlayerByRole(Role.Medic).Location;
                    Debug.Assert(CityByName(medicLocation).Cubes.NumberOf(curedColour) == 0);
                }
            }

            foreach (var curedColour in ColourExtensions.AllColours.Where(IsCured))
            {
                var numCubesOnCities = Cities.Sum(c => c.Cubes.NumberOf(curedColour));

                if (numCubesOnCities == 0)
                    Debug.Assert(IsEradicated(curedColour));
            }
        }

        public ISpecialEventCard? ContingencyPlannerStoredCard => Players
            .Where(p => p.Role == Role.ContingencyPlanner)
            .Select(p => (ContingencyPlanner)p)
            .Select(p => p.StoredEventCard)
            .FirstOrDefault();

        private bool APlayerHasASpecialEventCard => Players.Any(p => p.Hand.Cards.Any(c => c is ISpecialEventCard)) ||
                                                    ContingencyPlannerStoredCard != null;

        private int TotalCubesInGame()
        {
            var totalCubes = 0;

            totalCubes += Cubes.NumberOf(Colour.Black);
            totalCubes += Cubes.NumberOf(Colour.Blue);
            totalCubes += Cubes.NumberOf(Colour.Red);
            totalCubes += Cubes.NumberOf(Colour.Yellow);

            foreach (var city in Cities)
            {
                totalCubes += city.Cubes.NumberOf(Colour.Black);
                totalCubes += city.Cubes.NumberOf(Colour.Blue);
                totalCubes += city.Cubes.NumberOf(Colour.Red);
                totalCubes += city.Cubes.NumberOf(Colour.Yellow);
            }

            return totalCubes;
        }
    }
}
