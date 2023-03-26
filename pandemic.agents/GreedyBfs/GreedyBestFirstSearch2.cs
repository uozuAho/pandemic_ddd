using System;
using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;

namespace pandemic.agents.GreedyBfs
{
    /// <summary>
    /// Search for a win by greedily picking the next 'best' state, as determined
    /// by the <see cref="GameEvaluator"/>. Seems to get stuck in 'local maxima',
    /// where a couple of cures have been found, but can't get to a win.
    ///
    /// Ideas:
    /// - take more game aspects into account in GameEvaluator, eg.
    ///     - prefer earlier cures
    ///     - count cards, don't explore states that can't win
    ///     - prefer being near/on research stations
    /// - visualise how this agent is getting stuck. Is it searching where
    ///   it shouldn't be?
    /// </summary>
    public class GreedyBestFirstSearch2
    {
        public bool IsFinished { get; private set; }
        public bool IsSolved { get; private set; }
        public PandemicGame CurrentState { get; private set; }

        public MinPriorityFrontier Frontier;

        private readonly Dictionary<PandemicGame, SearchNode> _explored;

        public GreedyBestFirstSearch2(PandemicGame game)
        {
            CurrentState = game;
            _explored = new Dictionary<PandemicGame, SearchNode>();

            IsFinished = false;

            var nodeComparer = new SearchNodeComparer(CompareStates);
            Frontier = new MinPriorityFrontier(nodeComparer);
            var root = new SearchNode(game, null, default, 0,
                GameEvaluator.Score(game));
            Frontier.Push(root);
        }

        public SearchNode? Step()
        {
            if (IsFinished) return null;

            SearchNode node;

            do
            {
                if (Frontier.IsEmpty())
                {
                    IsFinished = true;
                    return null;
                }
                node = Frontier.Pop();
            } while (_explored.ContainsKey(node.State));

            _explored[node.State] = node;
            CurrentState = node.State;
            var commands = node.State.LegalCommands();
            foreach (var command in commands)
            {
                var (childState, _) = node.State.Do(command);
                var childCost = node.PathCost;
                var child = new SearchNode(childState, node, command, childCost, GameEvaluator.Score(childState));

                if (_explored.ContainsKey(childState) || Frontier.ContainsState(childState)) continue;

                if (childState.IsWon)
                {
                    CurrentState = childState;
                    IsFinished = true;
                    IsSolved = true;
                    // add goal to explored to allow this.getSolutionTo(goal)
                    _explored[childState] = child;
                }
                else
                {
                    Frontier.Push(child);
                }
            }

            return node;
        }

        protected class SearchNodeComparer : IComparer<SearchNode>
        {
            private readonly Func<SearchNode, SearchNode, int> _compare;

            public SearchNodeComparer(Func<SearchNode, SearchNode, int> compare)
            {
                _compare = compare;
            }

            public int Compare(
                SearchNode? x,
                SearchNode? y)
            {
                if (x == null) throw new NullReferenceException(nameof(x));
                if (y == null) throw new NullReferenceException(nameof(y));

                return _compare(x, y);
            }
        }

        protected int CompareStates(SearchNode a, SearchNode b)
        {
            var priorityA = (double)-a.Score;
            var priorityB = (double)-b.Score;
            return priorityA < priorityB ? -1 : priorityA > priorityB ? 1 : 0;
        }
    }
}
