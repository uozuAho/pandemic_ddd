using System;
using System.Collections.Generic;
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
        public List<Player> Players { get; init; } = new();
        public List<InfectionCard> InfectionDrawPile { get; set; } = new();
        public List<InfectionCard> InfectionDiscardPile { get; set; } = new();
        public Player CurrentPlayer => Players[CurrentPlayerIdx];
        public int CurrentPlayerIdx { get; init; } = 0;

        public Player PlayerByRole(Role role) => Players.Single(p => p.Role == role);

        private static readonly Board Board = new();

        public static PandemicGame FromEvents(IEnumerable<IEvent> events) =>
            events.Aggregate(new PandemicGame(), Apply);

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
            var unshuffledCities = Board.Cities.Select(c => new InfectionCard {City = c.Name});

            yield return new InfectionDeckSetUp(unshuffledCities);
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
            if (!Board.IsAdjacent(player.Location, city))
            {
                throw new InvalidActionException(
                    $"Invalid drive/ferry to non-adjacent city: {player.Location} to {city}");
            }

            yield return new PlayerMoved(role, city);

            if (player.ActionsRemaining == 1)
            {
                // todo: only pick up on end of turn
                // todo: pick up cards from player draw pile here
                yield return new PlayerCardPickedUp(role, new PlayerCard("Atlanta"));
                yield return new PlayerCardPickedUp(role, new PlayerCard("Atlanta"));
            }
        }

        private static PandemicGame Apply(PandemicGame game, IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => game with {Difficulty = d.Difficulty},
                InfectionDeckSetUp s => game with {InfectionDrawPile = s.Deck},
                InfectionRateSet i => game with {InfectionRate = i.Rate},
                OutbreakCounterSet o => game with {OutbreakCounter = o.Value},
                PlayerAdded p => ApplyPlayerAdded(game, p),
                PlayerMoved p => ApplyPlayerMoved(game, p),
                PlayerCardPickedUp p => ApplyPlayerCardPickedUp(game, p),
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
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

            newPlayers[currentPlayerIdx] = newPlayers[currentPlayerIdx] with {Hand = currentPlayerHand};

            return game with {Players = newPlayers};
        }

        private static PandemicGame ApplyPlayerAdded(PandemicGame pandemicGame, PlayerAdded playerAdded)
        {
            var newPlayers = pandemicGame.Players.Select(p => p).ToList();
            newPlayers.Add(new Player {Role = playerAdded.Role, Location = "Atlanta"});

            return pandemicGame with { Players = newPlayers };
        }

        private static PandemicGame ApplyPlayerMoved(PandemicGame pandemicGame, PlayerMoved playerMoved)
        {
            var newPlayers = pandemicGame.Players.Select(p => p).ToList();
            var movedPlayerIdx = newPlayers.FindIndex(p => p.Role == playerMoved.Role);
            var movedPlayer = newPlayers[movedPlayerIdx];

            // todo: check has actions remaining here

            newPlayers[movedPlayerIdx] = movedPlayer with
            {
                Location = playerMoved.Location,
                ActionsRemaining = movedPlayer.ActionsRemaining - 1
            };

            return pandemicGame with {Players = newPlayers};
        }
    }
}
