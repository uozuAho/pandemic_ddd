using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Events;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.Aggregates
{
    public record PandemicGame
    {
        public bool IsOver { get; init; } = false;
        public Difficulty Difficulty { get; init; }
        public int InfectionRate { get; init; }
        public int OutbreakCounter { get; init; }
        public Player CurrentPlayer => Players[CurrentPlayerIdx];
        public int CurrentPlayerIdx { get; init; } = 0;
        public ImmutableList<Player> Players { get; init; } = ImmutableList<Player>.Empty;
        public ImmutableList<City> Cities { get; init; }
        public ImmutableList<PlayerCard> PlayerDrawPile { get; init; }
        public ImmutableList<InfectionCard> InfectionDrawPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public ImmutableList<InfectionCard> InfectionDiscardPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public ImmutableDictionary<Colour, int> Cubes { get; init; } =
            Enum.GetValues<Colour>().ToImmutableDictionary(c => c, _ => 24);

        public readonly StandardGameBoard Board = new();

        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);
        public City CityByName(string city) => Cities.Single(c => c.Name == city);

        public bool IsSameStateAs(PandemicGame other)
        {
            if (IsOver != other.IsOver) return false;
            if (Difficulty != other.Difficulty) return false;
            if (InfectionRate != other.InfectionRate) return false;
            if (OutbreakCounter != other.OutbreakCounter) return false;
            if (CurrentPlayerIdx != other.CurrentPlayerIdx) return false;

            // order is expected to be the same and significant (different order means not equal)
            if (!Players.SequenceEqual(other.Players, Player.DefaultEqualityComparer)) return false;
            if (!Cities.SequenceEqual(other.Cities, City.DefaultEqualityComparer)) return false;
            if (!InfectionDrawPile.SequenceEqual(other.InfectionDrawPile)) return false;
            if (!InfectionDiscardPile.SequenceEqual(other.InfectionDiscardPile)) return false;
            if (!PlayerDrawPile.SequenceEqual(other.PlayerDrawPile)) return false;
            if (!Cubes.SequenceEqual(other.Cubes)) return false;

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

        private PandemicGame()
        {
            Cities = Board.Cities.Select(c => new City(c.Name)).ToImmutableList();
            PlayerDrawPile = Board.Cities
                .Select(c => new PlayerCityCard(c) as PlayerCard).ToImmutableList();
        }

        public static PandemicGame CreateUninitialisedGame() => new ();

        public static PandemicGame FromEvents(IEnumerable<IEvent> events) =>
            events.Aggregate(CreateUninitialisedGame(), ApplyEvent);

        public static (PandemicGame, List<IEvent>) CreateNewGame(NewGameOptions options)
        {
            var game = CreateUninitialisedGame();
            var events = new List<IEvent>();

            if (options.Roles.Count < 2 || options.Roles.Count > 4)
                throw new GameRuleViolatedException(
                    $"number of players must be between 2-4. Was given {options.Roles.Count}");

            game = game
                .SetDifficulty(options.Difficulty, events)
                .SetInfectionRate(2, events)
                .SetOutbreakCounter(0, events)
                .SetupInfectionDeck(events)
                .SetupPlayerDrawPile(events);

            // todo: infect cities

            foreach (var role in options.Roles)
            {
                game = game.AddPlayer(role, events);
                game = game.DealPlayerCards(role, InitialPlayerHandSize(options.Roles.Count), events);
            }

            return (game, events);
        }

        public override string ToString()
        {
            return PandemicGameStringRenderer.ToString(this);
        }

        // oh god I'm using regions! what have I become...
        #region Commands
        public (PandemicGame, ICollection<IEvent>) DriveOrFerryPlayer(Role role, string city)
        {
            ThrowIfGameOver(this);
            ThrowIfNotRolesTurn(role);
            ThrowIfNoActionsRemaining(CurrentPlayer);
            ThrowIfPlayerMustDiscard(CurrentPlayer);

            var player = PlayerByRole(role);

            if (!Board.IsCity(city)) throw new InvalidActionException($"Invalid city '{city}'");

            if (!Board.IsAdjacent(player.Location, city))
            {
                throw new InvalidActionException(
                    $"Invalid drive/ferry to non-adjacent city: {player.Location} to {city}");
            }

            var (currentState, events) = ApplyEvents(new PlayerMoved(role, city));

            if (currentState.CurrentPlayer.ActionsRemaining == 0)
                currentState = DoStuffAfterActions(currentState, events);

            return (currentState, events);
        }

        public (PandemicGame, IEnumerable<IEvent>) DiscardPlayerCard(PlayerCard card)
        {
            ThrowIfGameOver(this);

            var (game, events) = ApplyEvents(new PlayerCardDiscarded(card));

            if (CurrentPlayer.ActionsRemaining == 0)
                game = InfectCities(game, events);

            return (game, events);
        }

        public (PandemicGame Game, IEnumerable<IEvent> events) BuildResearchStation(string city)
        {
            ThrowIfGameOver(this);
            ThrowIfNoActionsRemaining(CurrentPlayer);
            ThrowIfPlayerMustDiscard(CurrentPlayer);

            return ApplyEvents(new ResearchStationBuilt(city));
        }

        private static PandemicGame InfectCities(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            game = InfectCity(game, events);
            if (!game.IsOver) game = InfectCity(game, events);
            if (!game.IsOver) game = game.ApplyEvent(new TurnEnded(), events);
            return game;
        }

        private PandemicGame SetDifficulty(Difficulty difficulty, ICollection<IEvent> events)
        {
            return ApplyEvent(new DifficultySet(difficulty), events);
        }

        private PandemicGame SetInfectionRate(int rate, ICollection<IEvent> events)
        {
            return ApplyEvent(new InfectionRateSet(rate), events);
        }

        private PandemicGame SetOutbreakCounter(int value, ICollection<IEvent> events)
        {
            return ApplyEvent(new OutbreakCounterSet(value), events);
        }

        private PandemicGame SetupInfectionDeck(ICollection<IEvent> events)
        {
            // todo: shuffle
            var unshuffledCities = Board.Cities.Select(c => new InfectionCard(c));

            return ApplyEvent(new InfectionDeckSetUp(unshuffledCities.ToImmutableList()), events);
        }

        private PandemicGame AddPlayer(Role role, ICollection<IEvent> events)
        {
            return ApplyEvent(new PlayerAdded(role), events);
        }

        private PandemicGame SetupPlayerDrawPile(ICollection<IEvent> events)
        {
            // todo: shuffle
            var playerCards = Board.Cities
                .Select(c => new PlayerCityCard(c) as PlayerCard)
                .Concat(Enumerable.Repeat(new EpidemicCard(), NumberOfEpidemicCards(Difficulty)))
                .ToImmutableList();

            return ApplyEvent(new PlayerDrawPileSetUp(playerCards), events);
        }

        private static PandemicGame DoStuffAfterActions(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            if (!game.PlayerDrawPile.Any())
                return game.ApplyEvent(new GameLost("No more player cards"), events);

            game = PickUpCard(game, events);

            if (!game.PlayerDrawPile.Any())
                return game.ApplyEvent(new GameLost("No more player cards"), events);

            game = PickUpCard(game, events);

            if (game.CurrentPlayer.Hand.Count > 7)
                return game;

            game = InfectCities(game, events);

            return game;
        }

        private static PandemicGame PickUpCard(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            return game.ApplyEvent(new PlayerCardPickedUp(), events);
        }

        private static PandemicGame InfectCity(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            if (game.InfectionDrawPile.Count == 0)
                return game.ApplyEvent(new GameLost("Ran out of infection cards"), events);

            var infectionCard = game.InfectionDrawPile.Last();
            game = game.ApplyEvent(new InfectionCardDrawn(infectionCard.City), events);

            return game.Cubes[infectionCard.City.Colour] == 0
                ? game.ApplyEvent(new GameLost($"Ran out of {infectionCard.City.Colour} cubes"), events)
                : game.ApplyEvent(new CubeAddedToCity(infectionCard.City), events);
        }

        private static void ThrowIfGameOver(PandemicGame game)
        {
            if (game.IsOver) throw new GameRuleViolatedException("Game is over!");
        }

        private void ThrowIfNotRolesTurn(Role role)
        {
            if (CurrentPlayer.Role != role) throw new GameRuleViolatedException($"It's not {role}'s turn!");
        }

        private static void ThrowIfNoActionsRemaining(Player player)
        {
            if (player.ActionsRemaining == 0)
                throw new GameRuleViolatedException($"Action not allowed: Player {player.Role} has no actions remaining");
        }

        private static void ThrowIfPlayerMustDiscard(Player player)
        {
            if (player.Hand.Count > 7)
                throw new GameRuleViolatedException($"Action not allowed: Player {player.Role} has more than 7 cards in hand");
        }

        #endregion

        #region Events
        private (PandemicGame, ICollection<IEvent>) ApplyEvents(params IEvent[] events)
        {
            var state = events.Aggregate(this, ApplyEvent);
            return (state, events.ToList());
        }

        private PandemicGame ApplyEvent(IEvent @event, ICollection<IEvent> events)
        {
            events.Add(@event);
            return ApplyEvent(this, @event);
        }

        private static PandemicGame ApplyEvent(PandemicGame game, IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => game with {Difficulty = d.Difficulty},
                InfectionCardDrawn i => ApplyInfectionCardDrawn(game, i),
                InfectionDeckSetUp s => game with {InfectionDrawPile = s.Deck.ToImmutableList()},
                InfectionRateSet i => game with {InfectionRate = i.Rate},
                OutbreakCounterSet o => game with {OutbreakCounter = o.Value},
                PlayerAdded p => ApplyPlayerAdded(game, p),
                PlayerMoved p => ApplyPlayerMoved(game, p),
                ResearchStationBuilt r => ApplyResearchStationBuilt(game, r),
                PlayerCardPickedUp p => ApplyPlayerCardPickedUp(game),
                PlayerCardsDealt d => ApplyPlayerCardsDealt(game, d),
                PlayerDrawPileSetUp p => ApplyPlayerDrawPileSetUp(game, p),
                PlayerCardDiscarded p => ApplyPlayerCardDiscarded(game, p),
                CubeAddedToCity c => ApplyCubesAddedToCity(game, c),
                GameLost g => game with { IsOver = true },
                TurnEnded t => ApplyTurnEnded(game),
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }

        private static PandemicGame ApplyResearchStationBuilt(PandemicGame game, ResearchStationBuilt @event)
        {
            var city = game.Cities.Single(c => c.Name == @event.City);
            // todo: hand needs a citycards method
            var playerCard = game.CurrentPlayer.Hand
                .Where(c => c is PlayerCityCard).Cast<PlayerCityCard>()
                .Single(c => c.City.Name == @event.City);

            return game with
            {
                Cities = game.Cities.Replace(city, city with
                {
                    HasResearchStation = true
                }),
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = game.CurrentPlayer.Hand.Remove(playerCard)
                })
            };
        }

        private static PandemicGame ApplyPlayerDrawPileSetUp(PandemicGame game, PlayerDrawPileSetUp @event)
        {
            return game with
            {
                PlayerDrawPile = @event.Pile
            };
        }

        private static PandemicGame ApplyPlayerCardsDealt(PandemicGame game, PlayerCardsDealt dealt)
        {
            var (role, numCards) = dealt;
            var cards = game.PlayerDrawPile.TakeLast(numCards).ToList();
            var player = game.PlayerByRole(role);

            return game with
            {
                PlayerDrawPile = game.PlayerDrawPile.RemoveRange(cards),
                Players = game.Players.Replace(player, player with
                {
                    Hand = cards.ToImmutableList()
                })
            };
        }

        private static PandemicGame ApplyInfectionCardDrawn(PandemicGame game, InfectionCardDrawn infectionCardDrawn)
        {
            return game with
            {
                InfectionDrawPile = game.InfectionDrawPile.RemoveAt(game.InfectionDrawPile.Count - 1),
                InfectionDiscardPile = game.InfectionDiscardPile.Add(new InfectionCard(infectionCardDrawn.City)),
            };
        }

        private static PandemicGame ApplyCubesAddedToCity(PandemicGame game, CubeAddedToCity cubeAddedToCity)
        {
            var city = game.CityByName(cubeAddedToCity.City.Name);
            var colour = cubeAddedToCity.City.Colour;
            var newCity = city with { Cubes = city.Cubes.SetItem(colour, city.Cubes[colour] + 1) };

            return game with
            {
                Cities = game.Cities.Replace(city, newCity),
                Cubes = game.Cubes.SetItem(colour, game.Cubes[colour] - 1)
            };
        }

        private static PandemicGame ApplyPlayerCardPickedUp(PandemicGame game)
        {
            var pickedCard = game.PlayerDrawPile.Last();
            return game with
            {
                PlayerDrawPile = game.PlayerDrawPile.Remove(pickedCard),
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = game.CurrentPlayer.Hand.Add(pickedCard)
                })
            };
        }

        private static PandemicGame ApplyPlayerCardDiscarded(PandemicGame game, PlayerCardDiscarded discarded)
        {
            return game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = game.CurrentPlayer.Hand.Remove(discarded.Card)
                })
            };
        }

        private static PandemicGame ApplyPlayerAdded(PandemicGame pandemicGame, PlayerAdded playerAdded)
        {
            var newPlayers = pandemicGame.Players.Select(p => p with { }).ToList();
            newPlayers.Add(new Player {Role = playerAdded.Role, Location = "Atlanta"});

            return pandemicGame with { Players = newPlayers.ToImmutableList() };
        }

        private PandemicGame DealPlayerCards(Role role, int numCards, ICollection<IEvent> events)
        {
            return ApplyEvent(new PlayerCardsDealt(role, numCards), events);
        }

        private static PandemicGame ApplyPlayerMoved(PandemicGame pandemicGame, PlayerMoved playerMoved)
        {
            var newPlayers = pandemicGame.Players.Select(p => p).ToList();
            var movedPlayerIdx = newPlayers.FindIndex(p => p.Role == playerMoved.Role);
            var movedPlayer = newPlayers[movedPlayerIdx];

            newPlayers[movedPlayerIdx] = movedPlayer with
            {
                Location = playerMoved.Location,
                ActionsRemaining = movedPlayer.ActionsRemaining - 1
            };

            return pandemicGame with {Players = newPlayers.ToImmutableList()};
        }

        private static PandemicGame ApplyTurnEnded(PandemicGame game)
        {
            return game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with {ActionsRemaining = 4}),
                CurrentPlayerIdx = (game.CurrentPlayerIdx + 1) % game.Players.Count
            };
        }
        #endregion
    }
}
