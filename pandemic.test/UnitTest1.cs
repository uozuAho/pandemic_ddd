using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace pandemic.test
{
    public class GameSetup
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var game = Pandemic.NewGame();

            game.SetDifficulty(Difficulty.Normal);

            Assert.AreEqual(Difficulty.Normal, game.CurrentState.Difficulty);
        }
    }

    public enum Difficulty
    {
        Introductory,
        Normal,
        Heroic
    }

    public class Pandemic
    {
        public static PandemicGame NewGame()
        {
            return new PandemicGame();
        }

        public static IEnumerable<IEvent> SetDifficulty(List<IEvent> log, Difficulty difficulty)
        {
            yield return new DifficultySet(difficulty);
        }
    }

    public record DifficultySet : IEvent
    {
        public Difficulty Difficulty { get; }

        public DifficultySet(Difficulty difficulty)
        {
            Difficulty = difficulty;
        }
    }

    public class PandemicGame
    {
        public PandemicGameState CurrentState => Fold(_eventLog);
        private readonly List<IEvent> _eventLog = new();

        public void SetDifficulty(Difficulty difficulty)
        {
            _eventLog.AddRange(Pandemic.SetDifficulty(_eventLog, difficulty));
        }

        private static PandemicGameState Fold(IEnumerable<IEvent> eventLog)
        {
            var initialState = new PandemicGameState();

            return eventLog.Aggregate(initialState, (current, @event) => current.Apply(@event));
        }
    }

    public record PandemicGameState
    {
        public Difficulty Difficulty { get; init; }

        public PandemicGameState Apply(IEvent @event)
        {
            return @event switch
            {
                DifficultySet d => this with {Difficulty = d.Difficulty},
                _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
            };
        }
    }

    public interface IEvent
    {
    }
}
