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

        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);
        public City CityByName(string city) => Cities.Single(c => c.Name == city);

        public readonly StandardGameBoard Board = new();

        private PandemicGame()
        {
            Cities = Board.Cities.Select(c => new City(c.Name)).ToImmutableList();
            PlayerDrawPile = Board.Cities.Select(c => new PlayerCityCard(c.Name) as PlayerCard).ToImmutableList();
        }

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
            if (!Cubes.SequenceEqual(other.Cubes)) return false;

            return true;
        }

        public static PandemicGame CreateUninitialisedGame() => new ();

        public static PandemicGame FromEvents(IEnumerable<IEvent> events) =>
            events.Aggregate(CreateUninitialisedGame(), ApplyEvent);

        public static (PandemicGame, List<IEvent>) CreateNewGame(NewGameOptions options)
        {
            var game = CreateUninitialisedGame();
            var events = new List<IEvent>();
            ICollection<IEvent> tempEvents;

            if (options.Roles.Count < 2 || options.Roles.Count > 4)
                throw new GameRuleViolatedException(
                    $"number of players must be between 2-4. Was given {options.Roles.Count}");

            game = game
                .SetDifficulty(options.Difficulty, events)
                .SetInfectionRate(2, events);
            (game, tempEvents) = game.SetOutbreakCounter(0);
            events.AddRange(tempEvents);
            (game, tempEvents) = game.SetupInfectionDeck();
            events.AddRange(tempEvents);

            // todo: setup draw pile correctly
            game = game with {PlayerDrawPile = game.PlayerDrawPile.AddRange(Enumerable.Repeat(new EpidemicCard(), 5))};

            foreach (var role in options.Roles)
            {
                (game, tempEvents) = game.AddPlayer(role);
                events.AddRange(tempEvents);
                game = game.DealPlayerCards(role, 4, events);
            }

            return (game, events);
        }

        public override string ToString()
        {
            return PandemicGameStringRenderer.ToString(this);
        }

        // oh god I'm using regions! what have I become...
        #region Commands
        // todo: these setup commands dont need to be public
        private PandemicGame SetDifficulty(Difficulty difficulty, ICollection<IEvent> events)
        {
            return ApplyEvent(new DifficultySet(difficulty), events);
        }

        private PandemicGame SetInfectionRate(int rate, ICollection<IEvent> events)
        {
            return ApplyEvent(new InfectionRateSet(rate), events);
        }

        private (PandemicGame, ICollection<IEvent>) SetOutbreakCounter(int value)
        {
            return ApplyEvents(new OutbreakCounterSet(value));
        }

        private (PandemicGame, ICollection<IEvent>) SetupInfectionDeck()
        {
            // todo: shuffle
            var unshuffledCities = Board.Cities.Select(c => new InfectionCard(c));

            return ApplyEvents(new InfectionDeckSetUp(unshuffledCities.ToImmutableList()));
        }

        private (PandemicGame, ICollection<IEvent>) AddPlayer(Role role)
        {
            return ApplyEvents(new PlayerAdded(role));
        }

        public (PandemicGame, ICollection<IEvent>) DriveOrFerryPlayer(Role role, string city)
        {
            ThrowIfGameOver(this);
            if (CurrentPlayer.Role != role) throw new GameRuleViolatedException($"It's not {role}'s turn!");

            if (!Board.IsCity(city)) throw new InvalidActionException($"Invalid city '{city}'");

            var player = PlayerByRole(role);

            if (player.ActionsRemaining == 0)
                throw new GameRuleViolatedException($"Action not allowed: Player {role} has no actions remaining");

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
            var (game, events) = ApplyEvents(new PlayerCardDiscarded(card));

            if (CurrentPlayer.ActionsRemaining == 0)
            {
                // todo: extract this, use in DoStuffAfterActions
                game = InfectCity(game, events);

                if (!game.IsOver) game = InfectCity(game, events);

                if (!game.IsOver) game = game.ApplyEvent(new TurnEnded(), events);
            }

            return (game, events);
        }

        private static PandemicGame DoStuffAfterActions(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            if (!game.PlayerDrawPile.Any()) return ApplyEvent(game, new GameLost("No more player cards"));
            game = PickUpCard(game, events);

            if (!game.PlayerDrawPile.Any()) return ApplyEvent(game, new GameLost("No more player cards"));
            game = PickUpCard(game, events);

            if (game.CurrentPlayer.Hand.Count > 7)
                return game;

            game = InfectCity(game, events);

            if (!game.IsOver) game = InfectCity(game, events);

            if (!game.IsOver) game = game.ApplyEvent(new TurnEnded(), events);

            return game;
        }

        private static PandemicGame PickUpCard(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            return game.ApplyEvent(new PlayerCardPickedUp(game.CurrentPlayer.Role), events);
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
                PlayerCardPickedUp p => ApplyPlayerCardPickedUp(game, p),
                PlayerCardsDealt d => ApplyPlayerCardsDealt(game, d),
                PlayerCardDiscarded p => ApplyPlayerCardDiscarded(game, p),
                CubeAddedToCity c => ApplyCubesAddedToCity(game, c),
                GameLost g => game with { IsOver = true },
                TurnEnded t => ApplyTurnEnded(game),
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
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

        private static PandemicGame ApplyPlayerCardPickedUp(
            PandemicGame game,
            PlayerCardPickedUp playerCardPickedUp)
        {
            // todo: make this shorter?
            var newPlayers = game.Players.Select(p => p).ToList();
            var currentPlayerIdx = newPlayers.FindIndex(p => p.Role == playerCardPickedUp.Role);
            var currentPlayerHand = newPlayers[currentPlayerIdx].Hand.Select(h => h).ToList();

            var card = game.PlayerDrawPile.Last();
            currentPlayerHand.Add(card);

            newPlayers[currentPlayerIdx] = newPlayers[currentPlayerIdx] with { Hand = currentPlayerHand.ToImmutableList() };

            return game with { Players = newPlayers.ToImmutableList(), PlayerDrawPile = game.PlayerDrawPile.Remove(card) };
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
