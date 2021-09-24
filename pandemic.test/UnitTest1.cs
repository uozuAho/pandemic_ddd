using NUnit.Framework;

namespace pandemic.test
{
    public class GameSetup
    {
        [Test]
        public void Set_difficulty_works()
        {
            var game = Pandemic.NewGame();

            game.SetDifficulty(Difficulty.Normal);

            Assert.AreEqual(Difficulty.Normal, game.CurrentState.Difficulty);
        }
    }
}
