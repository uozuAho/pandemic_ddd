using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace pandemic.agents
{
    /// <summary>
    /// Monte-Carlo Tree Search.
    /// I'm planning to copy https://github.com/deepmind/open_spiel/blob/master/open_spiel/python/algorithms/mcts.py
    /// </summary>
    internal class MctsAgent
    {
        public RandomRolloutEvaluator _evaluator;

        private readonly int _maxSimulations;
        private bool _solve = false;
        private const double _maxUtility = 1.0;

        public MctsAgent(int maxSimulations)
        {
            _maxSimulations = maxSimulations;
            _evaluator = new RandomRolloutEvaluator();
        }

        // done
        private PlayerCommand Step(PandemicSpielGameState state)
        {
            var root = MctsSearch(state);
            var best = root.BestChild();

            // how to remove need for all these null checks? Root node needs a null command, others don't
            if (best.Command == null) throw new InvalidOperationException("this shouldn't happen!");

            return best.Command;
        }

        // done
        private SearchNode MctsSearch(PandemicSpielGameState state)
        {
            var rootPlayer = state.CurrentPlayerIdx;
            var root = new SearchNode(state.CurrentPlayerIdx, null) { Prior = 1.0 };

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
                                allSolved = false;
                            // todo: remove null forgiving operator here
                            // it's here to stay true to the python code I'm copying from
                            else if (best == null || child.Outcomes[player] > best.Outcomes![player])
                                best = child;
                        }

                        if (best != null &&
                            (allSolved || best.Outcomes![player] == _maxUtility))
                            node.Outcomes = best.Outcomes;
                        else
                            solved = false;
                    }
                }

                if (root.Outcomes != null)
                    break;
            }

            return root;
        }

        // done
        private (List<SearchNode> visitPath, PandemicSpielGameState workingState)
            ApplyTreePolicy(SearchNode root, PandemicSpielGameState state)
        {
            var visitPath = new List<SearchNode> { root };
            var workingState = state.Clone();
            var currentNode = root;

            while (!workingState.IsTerminal && currentNode.ExploreCount > 0)
            {
                if (currentNode.IsLeaf)
                {
                    var legalActions = _evaluator.Prior(workingState).Shuffle();
                    currentNode.Children.AddRange(legalActions.Select(a =>
                        new SearchNode(workingState.CurrentPlayerIdx, a.Item1)
                        {
                            Prior = a.Item2
                        }
                    ));
                }

                if (!currentNode.Children.Any())
                    throw new InvalidOperationException("non terminal state must have children");

                var exploreCount = currentNode.ExploreCount;
                var chosenChild = currentNode.Children.MaxBy(c => c.UctValue(exploreCount))!;

                workingState.ApplyAction(chosenChild.Command!);
                currentNode = chosenChild;
                visitPath.Add(currentNode);
            }

            return (visitPath, workingState);
        }
    }

    // todo: move somewhere else
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            var rng = new Random();
            return items.OrderBy(_ => rng.Next());
        }

        public static IEnumerable<T> Reversed<T>(this IEnumerable<T> items)
        {
            var temp = new List<T>();
            temp.AddRange(items);
            temp.Reverse();
            return temp;
        }
    }

    internal class RandomRolloutEvaluator
    {
        private readonly PlayerCommandGenerator _commandGenerator = new();

        /// <summary>
        /// Returns (command, probability)
        /// </summary>
        public IEnumerable<(PlayerCommand, double)> Prior(PandemicSpielGameState state)
        {
            var legalActions = state.LegalActions().ToList();
            return legalActions.Select(a => (a, 1.0 / legalActions.Count));
        }

        public double[] Evaluate(PandemicSpielGameState workingState)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="PlayerIdx"></param>
    /// <param name="Command">The command that lead to this node</param>
    internal record SearchNode(int PlayerIdx, PlayerCommand? Command)
    {
        public int ExploreCount { get; set; }
        public double Prior { get; set; }
        public double[]? Outcomes { get; set; }
        public double TotalReward { get; set; }
        public List<SearchNode> Children { get; set; } = new();

        public bool IsLeaf => !Children.Any();

        public SearchNode BestChild()
        {
            if (!Children.Any()) throw new InvalidOperationException("No children");

            return Children.MaxBy(c => c.SortKey())!;
        }

        public double UctValue(int parentExploreCount)
        {
            if (Outcomes != null) return Outcomes[PlayerIdx];
            if (ExploreCount == 0) return double.PositiveInfinity;

            return TotalReward / ExploreCount + Math.Sqrt(2) * Math.Sqrt(
                Math.Log(parentExploreCount) / ExploreCount);
        }

        // https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/python/algorithms/mcts.py#L144
        private (double, int, double) SortKey()
        {
            var outcome = 0.0;
            if (Outcomes != null) outcome = Outcomes[PlayerIdx];

            return (outcome, ExploreCount, TotalReward);
        }
    }
}
