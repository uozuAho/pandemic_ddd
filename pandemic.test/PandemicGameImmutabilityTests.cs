using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.test
{
    internal class PandemicGameImmutabilityTests
    {
        [Test]
        public void Games_from_same_events_are_not_same()
        {
            var game1 = PandemicGame.CreateUninitialisedGame();
            var (game2, _) = game1.AddPlayer(Role.Medic);

            Assert.AreNotSame(game1, game2);
            Assert.AreNotEqual(game1, game2);
        }

        [Test]
        public void Player_list_is_not_shallow_copy()
        {
            var (game1, _) = PandemicGame
                .CreateUninitialisedGame()
                .AddPlayer(Role.Medic);
            var (game2, _) = game1.AddPlayer(Role.Medic);

            Assert.AreNotSame(game1.Players, game2.Players);
            Assert.AreNotEqual(game1.Players, game2.Players);
            Assert.AreNotSame(game1.Players[0], game2.Players[0]);
        }
    }
}
