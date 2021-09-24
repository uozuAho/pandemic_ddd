using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates
{
    public record PandemicGame
    {
        public Difficulty Difficulty { get; init; }
        public int InfectionRate { get; init; }
        public int OutbreakCounter { get; init; }
        public List<Player> Players { get; init; } = new();

        // private static readonly Board = // crap, need board

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

        public static IEnumerable<IEvent> AddPlayer(List<IEvent> log, Role role)
        {
            yield return new PlayerAdded(role);
        }

        public static IEnumerable<IEvent> DriveOrFerryPlayer(List<IEvent> log, Role role, string city)
        {
            // var state = FromEvents(log);
            //
            // state.PlayerByRole(role)
            yield return new PlayerMoved(role, city);
        }

        public Player PlayerByRole(Role role)
        {
            return Players.Single(p => p.Role == role);
        }

        private static PandemicGame Apply(PandemicGame pandemicGame, IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => pandemicGame with {Difficulty = d.Difficulty},
                InfectionRateSet i => pandemicGame with {InfectionRate = i.Rate},
                OutbreakCounterSet o => pandemicGame with {OutbreakCounter = o.Value},
                PlayerAdded p => ApplyPlayerAdded(pandemicGame, p),
                PlayerMoved p => ApplyPlayerMoved(pandemicGame, p),
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
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
            newPlayers[movedPlayerIdx] = newPlayers[movedPlayerIdx] with {Location = playerMoved.Location};

            return pandemicGame with {Players = newPlayers};
        }
    }
}
