namespace pandemic.agents;

using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using utils;

/// <summary>
/// Monte-Carlo Tree Search.
/// Copied from https://github.com/deepmind/open_spiel/blob/master/open_spiel/python/algorithms/mcts.py
/// </summary>
public class MctsAgent
{
    private readonly int _maxSimulations;
    private readonly bool _solve = true;
    private const double _maxUtility = 1.0;
    private readonly RandomRolloutEvaluator _evaluator;

    public MctsAgent(int maxSimulations, int numRollouts)
    {
        // this is the same quirk as in the OpenSpiel version. One sim means the root node is not
        // expanded, thus no options are evaluated!
        if (maxSimulations < 2)
        {
            throw new ArgumentException($"{nameof(maxSimulations)} must be > 1");
        }

        _maxSimulations = maxSimulations;
        _evaluator = new RandomRolloutEvaluator(numRollouts);
    }

    public int Step(PandemicSpielGameState state)
    {
        var root = MctsSearch(state);
        var best = root.BestChild();

        return best.Action;
    }

    private SearchNode MctsSearch(PandemicSpielGameState state)
    {
        _ = state.CurrentPlayerIdx;
        var root = new SearchNode { PlayerIdx = state.CurrentPlayerIdx, Prior = 1.0 };

        for (var i = 0; i < _maxSimulations; i++)
        {
            var (visitPath, workingState) = ApplyTreePolicy(root, state);
            double[] returns;
            var solved = false;

            if (workingState.IsTerminal)
            {
                returns = workingState.Returns;
                visitPath.Last().Outcomes = returns;
                solved = _solve;
            }
            else
            {
                returns = _evaluator.Evaluate(workingState);
                solved = false;
            }

            foreach (var node in visitPath.Reversed())
            {
                node.TotalReward += returns[node.PlayerIdx];
                node.ExploreCount++;

                if (solved && !node.IsLeaf)
                {
                    var player = node.PlayerIdx;
                    SearchNode? best = null;
                    var allSolved = true;

                    foreach (var child in node.Children)
                    {
                        if (child.Outcomes == null)
                        {
                            allSolved = false;
                        }
                        // todo: remove null forgiving operator here
                        // it's here to stay true to the python code I'm copying from
                        else if (best == null || child.Outcomes[player] > best.Outcomes![player])
                        {
                            best = child;
                        }
                    }

                    if (best != null && (allSolved || best.Outcomes![player] == _maxUtility))
                    {
                        node.Outcomes = best.Outcomes;
                    }
                    else
                    {
                        solved = false;
                    }
                }
            }

            if (root.Outcomes != null)
            {
                break;
            }
        }

        return root;
    }

    private static (List<SearchNode> visitPath, PandemicSpielGameState workingState) ApplyTreePolicy(
        SearchNode root,
        PandemicSpielGameState state
    )
    {
        var visitPath = new List<SearchNode> { root };
        var workingState = state.Clone();
        var currentNode = root;

        while (!workingState.IsTerminal && currentNode.ExploreCount > 0)
        {
            if (currentNode.IsLeaf)
            {
                var legalActions = RandomRolloutEvaluator.Prior(workingState).Shuffle();
                currentNode.Children.AddRange(
                    legalActions.Select(a => new SearchNode
                    {
                        PlayerIdx = workingState.CurrentPlayerIdx,
                        Action = a.Item1,
                        Prior = a.Item2,
                    })
                );
            }

            if (currentNode.Children.Count == 0)
            {
                throw new InvalidOperationException("non terminal state must have children");
            }

            var exploreCount = currentNode.ExploreCount;
            var chosenChild = currentNode.Children.MaxBy(c => c.UctValue(exploreCount))!;

            _ = workingState.ApplyAction(chosenChild.Action);
            currentNode = chosenChild;
            visitPath.Add(currentNode);
        }

        return (visitPath, workingState);
    }
}

internal class RandomRolloutEvaluator(int numRollouts)
{
    private readonly int _numRollouts = numRollouts;
    private readonly Random _random = new();

    /// <summary>
    /// Returns (command, probability)
    /// </summary>
    public static IEnumerable<(int, double)> Prior(PandemicSpielGameState state)
    {
        var legalActions = state.LegalActions().ToList();
        return Enumerable.Range(0, legalActions.Count).Select(i => (i, 1.0 / legalActions.Count));
    }

    public double[] Evaluate(PandemicSpielGameState state)
    {
        double[]? result = null;

        for (var i = 0; i < _numRollouts; i++)
        {
            var events = new List<IEvent>();
            var workingState = state.Clone();

            while (!workingState.IsTerminal)
            {
                var action = _random.Choice(workingState.LegalActions());
                events.AddRange(workingState.ApplyAction(action));
            }

            result =
                result == null
                    ? workingState.Returns
                    : result.Zip(workingState.Returns, (a, b) => a + b).ToArray();
        }

        if (result == null)
        {
            throw new InvalidOperationException("doh");
        }

        return result.Select(r => r / _numRollouts).ToArray();
    }
}

public class SearchNode
{
    public int PlayerIdx { get; set; }
    public int Action { get; set; }
    public int ExploreCount { get; set; }
    public double Prior { get; set; }

// todo: fix me later. properties shouldn't return arrays
#pragma warning disable CA1819
    public double[]? Outcomes { get; set; }
#pragma warning restore CA1819
    public double TotalReward { get; set; }

// todo: fix me later. don't use List<T>
#pragma warning disable CA1002
    public List<SearchNode> Children { get; init; } = [];
#pragma warning restore CA1002

    public bool IsLeaf => Children.Count == 0;

    public SearchNode BestChild()
    {
        if (Children.Count == 0)
        {
            throw new InvalidOperationException("No children");
        }

        return Children.MaxBy(c => c.SortKey())!;
    }

    public double UctValue(int parentExploreCount)
    {
        if (Outcomes != null)
        {
            return Outcomes[PlayerIdx];
        }

        if (ExploreCount == 0)
        {
            return double.PositiveInfinity;
        }

        return (TotalReward / ExploreCount)
            + (Math.Sqrt(2) * Math.Sqrt(Math.Log(parentExploreCount) / ExploreCount));
    }

    // https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/python/algorithms/mcts.py#L144
    private (double, int, double) SortKey()
    {
        var outcome = 0.0;
        if (Outcomes != null)
        {
            outcome = Outcomes[PlayerIdx];
        }

        return (outcome, ExploreCount, TotalReward);
    }
}
