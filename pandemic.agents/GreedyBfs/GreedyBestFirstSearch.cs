using System;
using System.Collections.Generic;
using pandemic.Aggregates;

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
    /// - visualise how this agent is getting stuck. Is it searching where
    ///   it shouldn't be?
    /// </summary>
    public class GreedyBestFirstSearch
    {
        public bool IsFinished { get; private set; }
        public bool IsSolved { get; private set; }
        public PandemicGame CurrentState { get; private set; }

        public MinPriorityFrontier Frontier;

        private readonly PandemicSearchProblem _problem;
        private readonly Dictionary<PandemicGame, SearchNode> _explored;

        public GreedyBestFirstSearch(PandemicSearchProblem problem)
        {
            _problem = problem;
            CurrentState = problem.InitialState;
            _explored = new Dictionary<PandemicGame, SearchNode>();

            IsFinished = false;

            var nodeComparer = new SearchNodeComparer(CompareStates);
            Frontier = new MinPriorityFrontier(nodeComparer);
            var root = new SearchNode(problem.InitialState, null, default, 0,
                GameEvaluator.Evaluate(problem.InitialState));
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
            var actions = _problem.GetActions(node.State);
            foreach (var action in actions)
            {
                var childState = _problem.DoAction(node.State, action);
                var childCost = node.PathCost + _problem.PathCost(node.State, action);
                var child = new SearchNode(childState, node, action, childCost, GameEvaluator.Evaluate(childState));

                if (_explored.ContainsKey(childState) || Frontier.ContainsState(childState)) continue;

                if (_problem.IsGoal(childState))
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

    /// <summary>
    /// </summary>
    /// <param name="State"></param>
    /// <param name="Parent"></param>
    /// <param name="Action">Action that resulted in this state. Arbitrary value if parent is null</param>
    /// <param name="PathCost">Path cost at this node = parent.path_cost + step_cost(parent, action)</param>
    public record SearchNode(
        PandemicGame State,
        SearchNode? Parent,
        PlayerCommand? Action,
        double PathCost,
        int Score);

    /// <summary>
    /// Search nodes are popped in minimum-priority order
    /// </summary>
    public class MinPriorityFrontier
    {
        private readonly MinPriorityQueue<SearchNode> _queue;
        private readonly HashSet<PandemicGame> _states;

        public int Size => _states.Count;

        public MinPriorityFrontier(IComparer<SearchNode> nodeComparer)
        {
            _queue = new MinPriorityQueue<SearchNode>(nodeComparer);
            _states = new HashSet<PandemicGame>();
        }

        public void Push(SearchNode node)
        {
            _states.Add(node.State);
            _queue.Push(node);
        }

        public SearchNode Pop()
        {
            var node = _queue.Pop();
            _states.Remove(node.State);
            return node;
        }

        public bool ContainsState(PandemicGame state)
        {
            return _states.Contains(state);
        }

        public bool IsEmpty()
        {
            return _states.Count == 0;
        }
    }

    public class MinPriorityQueue<T>
    {
        private readonly BinaryMinHeap<T> _minHeap;

        public MinPriorityQueue(IComparer<T> comparer)
        {
            _minHeap = new BinaryMinHeap<T>(comparer);
        }

        public void Push(T item)
        {
            _minHeap.Add(item);
        }

        public T Pop()
        {
            return _minHeap.RemoveMin();
        }
    }

    public class BinaryMinHeap<T>
    {
        public int Size => _buf.Count;

        private readonly List<T> _buf;
        private readonly IComparer<T> _comparer;

        public BinaryMinHeap(IComparer<T> comparer)
        {
            _buf = new List<T>();
            _comparer = comparer;
        }

        public void Add(T item)
        {
            _buf.Add(item);
            Swim(Size - 1);
        }

        public T RemoveMin()
        {
            if (Size == 0) throw new InvalidOperationException("cannot remove from empty");
            return RemoveAtIdx(0);
        }

        private T RemoveAtIdx(int idx)
        {
            if (idx >= Size) throw new ArgumentOutOfRangeException();

            // swap item at idx and last item
            var temp = _buf[idx];
            var lastIdx = Size - 1;
            _buf[idx] = _buf[lastIdx];
            _buf.RemoveAt(lastIdx);
            // sink last item placed at idx
            Sink(idx);
            return temp;
        }

        private void Swim(int idx)
        {
            if (idx == 0) return;

            var parentIdx = (idx - 1) / 2;

            while (_comparer.Compare(_buf[idx], _buf[parentIdx]) == -1)
            {
                // swap if child < parent
                Swap(idx, parentIdx);
                if (parentIdx == 0) return;
                idx = parentIdx;
                parentIdx = (idx - 1) / 2;
            }
        }

        private void Sink(int idx)
        {
            while (true)
            {
                var leftIdx = 2 * idx + 1;
                var rightIdx = 2 * idx + 2;
                // stop if no children
                if (leftIdx >= Size) return;
                // get minimum child
                var minIdx = leftIdx;
                if (rightIdx < Size && _comparer.Compare(_buf[rightIdx], _buf[leftIdx]) == -1)
                {
                    minIdx = rightIdx;
                }
                // swap if parent > min child
                if (_comparer.Compare(_buf[idx], _buf[minIdx]) == 1)
                {
                    Swap(idx, minIdx);
                }
                else
                {
                    return;
                }
                idx = minIdx;
            }
        }

        private void Swap(int idxA, int idxB)
        {
            (_buf[idxA], _buf[idxB]) = (_buf[idxB], _buf[idxA]);
        }
    }
}
