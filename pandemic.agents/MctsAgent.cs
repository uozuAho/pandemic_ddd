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
        private readonly int _maxSimulations;
        public RandomRolloutEvaluator _evaluator;

        public MctsAgent(int maxSimulations)
        {
            _maxSimulations = maxSimulations;
            _evaluator = new RandomRolloutEvaluator();
        }

        private PlayerCommand Step(PandemicSpielGameState state)
        {
            var root = MctsSearch(state);
            var best = root.BestChild();

            // how to remove need for all these null checks? Root node needs a null command, others don't
            if (best.Command == null) throw new InvalidOperationException("this shouldn't happen!");

            return best.Command;
        }

        private SearchNode MctsSearch(PandemicSpielGameState state)
        {
            var rootPlayer = state.CurrentPlayerIdx;
            var root = new SearchNode(state.CurrentPlayerIdx, null) { Prior = 1.0 };

            for (var i = 0; i < _maxSimulations; i++)
            {
                var (visitPath, workingState) = ApplyTreePolicy(root, state);
            }

            return root;
        }

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
                    currentNode.SetChildren(legalActions.Select(a =>
                        new SearchNode(workingState.CurrentPlayerIdx, a.Item1)
                        {
                            Prior = a.Item2
                        }
                    ));
                }

                if (currentNode.Children.IsEmpty)
                    throw new InvalidOperationException("non terminal state must have children");

                var chosenChild = currentNode.Children.MaxBy(c => c.UctValue())!;

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
    }

    /// <summary>
    /// </summary>
    /// <param name="PlayerIdx"></param>
    /// <param name="Command">The command that lead to this node</param>
    internal record SearchNode(int PlayerIdx, PlayerCommand? Command)
    {
        public int ExploreCount { get; init; }
        public double Prior { get; init; }
        public double[]? Outcomes { get; init; }
        public double TotalReward { get; init; }
        public ImmutableList<SearchNode> Children { get; private set; } = ImmutableList<SearchNode>.Empty;

        public bool IsLeaf => !Children.Any();

        public SearchNode BestChild()
        {
            if (Children.IsEmpty) throw new InvalidOperationException("No children");

            return Children.MaxBy(c => c.SortKey())!;
        }

        public void SetChildren(IEnumerable<SearchNode> children)
        {
            Children = children.ToImmutableList();
        }

        public double UctValue()
        {
            throw new NotImplementedException();
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
