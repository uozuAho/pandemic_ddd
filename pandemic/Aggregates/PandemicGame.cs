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

        public static PandemicGame FromEvents(IEnumerable<IEvent> events) =>
            events.Aggregate(new PandemicGame(), ApplyEvent);

        // oh god I'm using regions! what have I become...
        #region Commands
        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }

        public static IEnumerable<IEvent> SetInfectionRate(List<IEvent> log, int rate)
        {
            yield return new InfectionRateSet(rate);
        }

        public static IEnumerable<IEvent> SetOutbreakCounter(List<IEvent> log, int value)
        {
            yield return new OutbreakCounterSet(value);
        }

        public static IEnumerable<IEvent> SetupInfectionDeck(List<IEvent> eventLog)
        {
            // todo: shuffle
            var unshuffledCities = Board.Cities.Select(c => new InfectionCard(c.Name, c.Colour));

            yield return new InfectionDeckSetUp(unshuffledCities.ToImmutableList());
        }

        public static IEnumerable<IEvent> AddPlayer(List<IEvent> log, Role role)
        {
            yield return new PlayerAdded(role);
        }

        public static IEnumerable<IEvent> DriveOrFerryPlayer(List<IEvent> log, Role role, string city)
        {
            if (!Board.IsCity(city)) throw new InvalidActionException($"Invalid city '{city}'");

            var state = FromEvents(log);
            var player = state.PlayerByRole(role);

            if (player.ActionsRemaining == 0)
                throw new GameRuleViolatedException($"Action not allowed: Player {role} has no actions remaining");

            if (!Board.IsAdjacent(player.Location, city))
            {
                throw new InvalidActionException(
                    $"Invalid drive/ferry to non-adjacent city: {player.Location} to {city}");
            }

            yield return new PlayerMoved(role, city);

            if (player.ActionsRemaining == 1)
            {
                // todo: pick up cards from player draw pile here
                yield return new PlayerCardPickedUp(role, new PlayerCard("Atlanta"));
                yield return new PlayerCardPickedUp(role, new PlayerCard("Atlanta"));
                foreach (var @event in InfectCity(log))
                {
                    yield return @event;
                }
                foreach (var @event in InfectCity(log))
                {
                    yield return @event;
                }
            }
        }

        public static IEnumerable<IEvent> InfectCity(List<IEvent> log)
        {
            var state = FromEvents(log);
            var (city, colour) = state.InfectionDrawPile.Last();
            yield return new InfectionCardDrawn(city, colour);
            yield return new CubeAddedToCity(city, colour);
        }
        #endregion

        #region Events
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
                InfectionDiscardPile = game.InfectionDiscardPile.Add(new InfectionCard(infectionCardDrawn.City, infectionCardDrawn.Colour)),
            };
        }

        private static PandemicGame ApplyCubesAddedToCity(PandemicGame game, CubeAddedToCity cubeAddedToCity)
        {
            // todo: make cities a dictionary?
            var cities = game.Cities.ToDictionary(c => c.Name, c => c);
            var city = cities[cubeAddedToCity.City];
            var cubes = city.Cubes.ToDictionary(c => c.Key, c => c.Value);
            cubes[cubeAddedToCity.Colour] += 1;
            cities[cubeAddedToCity.City] = city with {Cubes = cubes.ToImmutableDictionary()};

            return game with
            {
                Cities = cities.Values.ToImmutableList()
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
            var newPlayers = pandemicGame.Players.Select(p => p).ToList();
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
