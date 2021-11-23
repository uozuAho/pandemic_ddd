using System.Linq;
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

    // up to here: https://github.com/deepmind/open_spiel/blob/c3d50a3fb3de7846bcba632619649b9f5d2f284c/open_spiel/python/algorithms/mcts_test.py#L161
    [Test]
    public void Chooses_most_visited_when_not_solved()
    {
        AssertBestChildIs(0,
            new SearchNode { Action = 0, ExploreCount = 50, TotalReward = 30 },
            new SearchNode { Action = 1, ExploreCount = 40, TotalReward = 40 });
    }

    [Test]
    public void Chooses_win_over_most_visited()
    {
        AssertBestChildIs(1,
            new SearchNode { Action = 0, ExploreCount = 50, TotalReward = 30 },
            new SearchNode { Action = 1, ExploreCount = 40, TotalReward = 40, Outcomes = new [] {1.0}});
    }

    private static void AssertBestChildIs(int choice, params SearchNode[] children)
    {
        children.Shuffle();
        var root = new SearchNode { Children = children.ToList() };
        Assert.AreEqual(root.BestChild().Action, choice);
    }
}
