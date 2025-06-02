namespace pandemic.agents.test;

using Aggregates.Game;
using NUnit.Framework;
using utils;
using Values;

public class MctsAgentTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Can_play_one_game()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist },
            }
        );
        var state = new PandemicSpielGameState(game);

        const int maxSimulations = 2;
        const int numRollouts = 2;
        var agent = new MctsAgent(maxSimulations, numRollouts);

        while (!state.IsTerminal)
        {
            var command = agent.Step(state);
            _ = state.ApplyAction(command);
        }

        Assert.IsTrue(state.IsTerminal);
    }

    // up to here: https://github.com/deepmind/open_spiel/blob/c3d50a3fb3de7846bcba632619649b9f5d2f284c/open_spiel/python/algorithms/mcts_test.py#L161
    [Test]
    public void Chooses_most_visited_when_not_solved()
    {
        AssertBestChildIs(
            0,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
            }
        );
    }

    [Test]
    public void Chooses_win_over_most_visited()
    {
        AssertBestChildIs(
            1,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
                Outcomes = [1.0],
            }
        );
    }

    [Test]
    public void Chooses_best_over_good()
    {
        AssertBestChildIs(
            1,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
                Outcomes = [0.5],
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
                Outcomes = [0.8],
            }
        );
    }

    [Test]
    public void Chooses_bad_over_worst()
    {
        AssertBestChildIs(
            0,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
                Outcomes = [-0.5],
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
                Outcomes = [-0.8],
            }
        );
    }

    [Test]
    public void Chooses_positive_reward_over_promising()
    {
        AssertBestChildIs(
            1,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 40,
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 10,
                TotalReward = 1,
                Outcomes = [0.1],
            }
        );
    }

    [Test]
    public void Chooses_most_visited_over_loss()
    {
        AssertBestChildIs(
            0,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
                Outcomes = [-1.0],
            }
        );
    }

    [Test]
    public void Chooses_most_visited_over_draw()
    {
        AssertBestChildIs(
            0,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
                Outcomes = [0.0],
            }
        );
    }

    [Test]
    public void Chooses_uncertainty_over_most_visited_loss()
    {
        AssertBestChildIs(
            1,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 30,
                Outcomes = [-1.0],
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 40,
                TotalReward = 40,
            }
        );
    }

    [Test]
    public void Chooses_slowest_loss()
    {
        AssertBestChildIs(
            1,
            new SearchNode
            {
                Action = 0,
                ExploreCount = 50,
                TotalReward = 10,
                Outcomes = [-1.0],
            },
            new SearchNode
            {
                Action = 1,
                ExploreCount = 60,
                TotalReward = 15,
                Outcomes = [-1.0],
            }
        );
    }

    private static void AssertBestChildIs(int choice, params SearchNode[] children)
    {
        _ = children.Shuffle();
        var root = new SearchNode { Children = [.. children] };
        Assert.AreEqual(root.BestChild().Action, choice);
    }
}
