namespace pandemic.test;

using Aggregates.Game;
using Commands;
using NUnit.Framework;
using Values;

internal class PandemicGameImmutabilityTests
{
    [Test]
    public void Applying_command_returns_cloned_state()
    {
        var (game1, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist },
            }
        );
        var (game2, _) = game1.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

        Assert.AreNotSame(game1, game2);
        Assert.AreNotEqual(game1, game2);
    }

    [Test]
    public void Player_list_is_not_shallow_copy()
    {
        var (game1, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist },
            }
        );
        var (game2, _) = game1.Do(new DriveFerryCommand(Role.Medic, "Chicago"));

        Assert.AreNotSame(game1.Players, game2.Players);
        Assert.AreNotEqual(game1.Players, game2.Players);
        Assert.AreNotSame(game1.Players[0], game2.Players[0]);
    }
}
