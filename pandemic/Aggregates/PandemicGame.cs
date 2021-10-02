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
        public Difficulty Difficulty { get; init; }
        public int InfectionRate { get; init; }
        public int OutbreakCounter { get; init; }
        public ImmutableList<Player> Players { get; init; } = ImmutableList<Player>.Empty;
        public ImmutableList<City> Cities { get; init; }
        public ImmutableList<InfectionCard> InfectionDrawPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public ImmutableList<InfectionCard> InfectionDiscardPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public Player CurrentPlayer => Players[CurrentPlayerIdx];
        public int CurrentPlayerIdx { get; init; } = 0;
        public bool IsOver { get; init; } = false;
        public ImmutableDictionary<Colour, int> Cubes { get; init; } =
            Enum.GetValues<Colour>().ToImmutableDictionary(c => c, _ => 24);

        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);
        public City CityByName(string city) => Cities.Single(c => c.Name == city);

        public readonly StandardGameBoard Board = new();

        private PandemicGame()
        {
            Cities = Board.Cities.Select(c => new City(c.Name)).ToImmutableList();
        }

        public static PandemicGame CreateUninitialisedGame() => new ();

        public static (PandemicGame, List<IEvent>) CreateNewGame(NewGameOptions options)
        {
            var game = CreateUninitialisedGame();
            var events = new List<IEvent>();
            ICollection<IEvent> tempEvents;

            if (options.Roles.Count < 2 || options.Roles.Count > 4)
                throw new GameRuleViolatedException(
                    $"number of players must be between 2-4. Was given {options.Roles.Count}");

            (game, tempEvents) = game.SetDifficulty(options.Difficulty);
            events.AddRange(tempEvents);
            (game, tempEvents) = game.SetInfectionRate(2);
            events.AddRange(tempEvents);
            (game, tempEvents) = game.SetOutbreakCounter(0);
            events.AddRange(tempEvents);
            (game, tempEvents) = game.SetupInfectionDeck();
            events.AddRange(tempEvents);
            foreach (var role in options.Roles)
            {
                (game, tempEvents) = game.AddPlayer(role);
                events.AddRange(tempEvents);
            }

            return (game, events);
        }

        public override string ToString()
        {
            return PandemicGameStringRenderer.ToString(this);
        }

        // oh god I'm using regions! what have I become...
        #region Commands
        public (PandemicGame, ICollection<IEvent>) SetDifficulty(Difficulty difficulty)
        {
            return ApplyEvents(new DifficultySet(difficulty));
        }

        public (PandemicGame, ICollection<IEvent>) SetInfectionRate(int rate)
        {
            return ApplyEvents(new InfectionRateSet(rate));
        }

        public (PandemicGame, ICollection<IEvent>) SetOutbreakCounter(int value)
        {
            return ApplyEvents(new OutbreakCounterSet(value));
        }

        public (PandemicGame, ICollection<IEvent>) SetupInfectionDeck()
        {
            // todo: shuffle
            var unshuffledCities = Board.Cities.Select(c => new InfectionCard(c));

            return ApplyEvents(new InfectionDeckSetUp(unshuffledCities.ToImmutableList()));
        }

        public (PandemicGame, ICollection<IEvent>) AddPlayer(Role role)
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

            if (player.ActionsRemaining == 1)
                currentState = DoStuffAfterActions(currentState, events);

            return (currentState, events);
        }

        private static PandemicGame DoStuffAfterActions(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            game = PickUpCard(game, events);
            game = PickUpCard(game, events);

            game = InfectCity(game, events);

            if (!game.IsOver) game = InfectCity(game, events);

            if (!game.IsOver) game = game.ApplyEvent(new TurnEnded(), events);

            return game;
        }

        private static PandemicGame PickUpCard(PandemicGame game, ICollection<IEvent> events)
        {
            ThrowIfGameOver(game);

            // todo: pick up cards from player draw pile here
            game = game.ApplyEvent(
                new PlayerCardPickedUp(game.CurrentPlayer.Role, new PlayerCard("Atlanta")),
                events);
            return game;
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
                CubeAddedToCity c => ApplyCubesAddedToCity(game, c),
                GameLost g => game with { IsOver = true },
                TurnEnded t => ApplyTurnEnded(game),
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
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
            var newPlayers = game.Players.Select(p => p).ToList();
            var currentPlayerIdx = newPlayers.FindIndex(p => p.Role == playerCardPickedUp.Role);
            var currentPlayerHand = newPlayers[currentPlayerIdx].Hand.Select(h => h).ToList();
            currentPlayerHand.Add(playerCardPickedUp.Card);

            newPlayers[currentPlayerIdx] = newPlayers[currentPlayerIdx] with { Hand = currentPlayerHand.ToImmutableList() };

            return game with {Players = newPlayers.ToImmutableList()};
        }

        private static PandemicGame ApplyPlayerAdded(PandemicGame pandemicGame, PlayerAdded playerAdded)
        {
            var newPlayers = pandemicGame.Players.Select(p => p with { }).ToList();
            newPlayers.Add(new Player {Role = playerAdded.Role, Location = "Atlanta"});

            return pandemicGame with { Players = newPlayers.ToImmutableList() };
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
