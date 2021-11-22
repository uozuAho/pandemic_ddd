using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.agents.test;

public class MctsAgentTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Can_play_one_game()
    {
        var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
        {
            Difficulty = Difficulty.Normal,
            Roles = new[] { Role.Medic, Role.Scientist }
        });
        var state = new PandemicSpielGameState(game);

        const int maxSimulations = 1;
        var agent = new MctsAgent(maxSimulations);

        while (!state.IsTerminal)
        {
            var command = agent.Step(state);
            state.ApplyAction(command);
        }

        Assert.IsTrue(state.IsTerminal);
    }
}
