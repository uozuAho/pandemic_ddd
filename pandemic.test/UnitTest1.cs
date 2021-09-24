using System.Collections.Generic;
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

        private PandemicGameState Fold(List<IEvent> eventLog)
        {
            var state = new PandemicGameState();

            foreach (var @event in eventLog)
            {
                state.Apply(@event);
            }

            return state;
        }

        private readonly List<IEvent> _eventLog = new();

        public void SetDifficulty(Difficulty difficulty)
        {
            _eventLog.AddRange(Pandemic.SetDifficulty(_eventLog, difficulty));
        }
    }

    public record PandemicGameState
    {
        public Difficulty Difficulty { get; }

        public void Apply(IEvent @event)
        {
        }
    }

    public interface IEvent
    {
    }
}
