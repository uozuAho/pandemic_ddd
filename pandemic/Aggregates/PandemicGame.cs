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
        public ImmutableList<City> Cities { get; init; } = Board.Cities.Select(c => new City(c.Name)).ToImmutableList();
        public ImmutableList<InfectionCard> InfectionDrawPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public ImmutableList<InfectionCard> InfectionDiscardPile { get; init; } = ImmutableList<InfectionCard>.Empty;
        public Player CurrentPlayer => Players[CurrentPlayerIdx];
        public int CurrentPlayerIdx { get; init; } = 0;

        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);
        public City CityByName(string city) => Cities.Single(c => c.Name == city);

        private static readonly Board Board = new();

        public static PandemicGame CreateUninitialisedGame() => new ();

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
                currentState = DoStuffAfterActions(player, currentState, events);

            return (currentState, events);
        }

        private PandemicGame DoStuffAfterActions(Player player, PandemicGame currentState, ICollection<IEvent> events)
        {
            currentState = PickUpCard(player, currentState, events);

            currentState = InfectCity(currentState, events);
            currentState = InfectCity(currentState, events);

            return currentState;
        }

        private static PandemicGame PickUpCard(Player player, PandemicGame currentState, ICollection<IEvent> events)
        {
            // todo: pick up cards from player draw pile here
            var (asdf, newEvents) = currentState.ApplyEvents(
                new PlayerCardPickedUp(player.Role, new PlayerCard("Atlanta")),
                new PlayerCardPickedUp(player.Role, new PlayerCard("Atlanta")));

            currentState = asdf;
            foreach (var @event in newEvents)
            {
                events.Add(@event);
            }

            return currentState;
        }

        private PandemicGame InfectCity(PandemicGame state, ICollection<IEvent> events)
        {
            var infectionCard = state.InfectionDrawPile.Last();
            state = state.ApplyEvent(new InfectionCardDrawn(infectionCard.City), events);
            state = state.ApplyEvent(new CubeAddedToCity(infectionCard.City), events);
            return state;
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
                Cities = game.Cities.Replace(city, newCity)
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
        #endregion
    }
}
