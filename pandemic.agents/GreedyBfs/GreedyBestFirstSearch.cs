using System;
using System.Collections.Generic;
using pandemic.Aggregates;

namespace pandemic.agents.GreedyBfs
{
    public class GreedyBestFirstSearch : BestFirstSearch
    {
        private readonly Func<PandemicGame, int> _heuristic;

        public GreedyBestFirstSearch(
            PandemicSearchProblem problem,
            Func<PandemicGame, int> heuristic) : base(problem)
        {
            _heuristic = heuristic;
            var root = new SearchNode<PandemicGame, PlayerCommand>(problem.InitialState, null, default, 0);
            Frontier.Push(root);
        }

        protected override double PriorityFunc(SearchNode<PandemicGame, PlayerCommand> node)
        {
            return _heuristic(node.State);
        }
    }

    public abstract class BestFirstSearch : GenericSearch<PandemicGame, PlayerCommand>
    {
        protected BestFirstSearch(PandemicSearchProblem problem) : base(problem)
        {
            var nodeComparer = new SearchNodeComparer(CompareStates);
            Frontier = new MinPriorityFrontier<PandemicGame, PlayerCommand>(nodeComparer);
        }

        /// <summary>
        /// The priority function determines in what order search nodes should be expanded.
        /// In the case of this best first search, nodes are expanded in lowest-value order.
        /// </summary>
        protected abstract double PriorityFunc(SearchNode<PandemicGame, PlayerCommand> node);

        private int CompareStates(SearchNode<PandemicGame, PlayerCommand> a, SearchNode<PandemicGame, PlayerCommand> b)
        {
            var priorityA = PriorityFunc(a);
            var priorityB = PriorityFunc(b);
            return priorityA < priorityB ? -1 : priorityA > priorityB ? 1 : 0;
        }

        private class SearchNodeComparer : IComparer<SearchNode<PandemicGame, PlayerCommand>>
        {
            private readonly Func<SearchNode<PandemicGame, PlayerCommand>, SearchNode<PandemicGame, PlayerCommand>, int> _compare;

            public SearchNodeComparer(Func<SearchNode<PandemicGame, PlayerCommand>, SearchNode<PandemicGame, PlayerCommand>, int> compare)
            {
                _compare = compare;
            }

            public int Compare(
                SearchNode<PandemicGame, PlayerCommand>? x,
                SearchNode<PandemicGame, PlayerCommand>? y)
            {
                if (x == null) throw new NullReferenceException(nameof(x));
                if (y == null) throw new NullReferenceException(nameof(y));

                return _compare(x, y);
            }
        }
    }

    public abstract class GenericSearch<TState, TAction>
    {
        public bool IsFinished { get; private set; }
        public bool IsSolved { get; private set; }
        public TState CurrentState { get; private set; }

        public ISearchFrontier<TState, TAction> Frontier;

        private readonly ISearchProblem<TState, TAction> _problem;
        private readonly Dictionary<TState, SearchNode<TState, TAction>> _explored;

        internal GenericSearch(ISearchProblem<TState, TAction> problem)
        {
            _problem = problem;
            CurrentState = problem.InitialState;
            _explored = new Dictionary<TState, SearchNode<TState, TAction>>();

            IsFinished = false;
        }

        public IEnumerable<TAction> GetSolutionTo(TState state)
        {
            if (!_explored.ContainsKey(state)) throw new ArgumentException("cannot get solution to unexplored state");

            var actions = new List<TAction>();
            var currentNode = _explored[state];
            while (currentNode != null && currentNode.Action != null)
            {
                actions.Add(currentNode.Action);
                currentNode = currentNode.Parent;
            }

            actions.Reverse();
            return actions;
        }

        public IEnumerable<TAction> GetSolution()
        {
            if (!IsSolved) throw new InvalidOperationException("No solution!");

            return GetSolutionTo(CurrentState);
        }

        public bool IsExplored(TState state)
        {
            return _explored.ContainsKey(state);
        }

        public SearchNode<TState, TAction>? Step()
        {
            if (IsFinished) return null;

            SearchNode<TState, TAction> node;

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
                var child = new SearchNode<TState, TAction>(childState, node, action, childCost);

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

        public void Solve()
        {
            while (!IsFinished)
            {
                Step();
            }
        }
    }

    public interface ISearchFrontier<TState, TAction>
    {
        void Push(SearchNode<TState, TAction> node);
        SearchNode<TState, TAction> Pop();
        bool ContainsState(TState state);
        bool IsEmpty();
        int Size { get; }
    }

    public class SearchNode<TState, TAction>
    {
        /// <summary>
        /// State at this node
        /// </summary>
        public TState State { get; }

        /// <summary>
        /// Action that resulted in this state. Arbitrary value if parent is null.
        /// </summary>
        public TAction? Action { get; }

        /// <summary>
        /// Previous state
        /// </summary>
        public SearchNode<TState, TAction>? Parent { get; }

        /// Path cost at this node = parent.path_cost + step_cost(parent, action) */
        public readonly double PathCost;

        public SearchNode(TState state,
            SearchNode<TState, TAction>? parent,
            TAction? action,
            double pathCost)
        {
            State = state;
            Parent = parent;
            Action = action;
            PathCost = pathCost;
        }
    }

    public interface ISearchProblem<TState, TAction>
    {
        TState InitialState { get; }

        /// <summary>
        /// Get available actions at the given state
        /// </summary>
        IEnumerable<TAction> GetActions(TState state);

        /// <summary>
        /// Do the given action and return the resultant state
        /// </summary>
        TState DoAction(TState state, TAction action);

        bool IsGoal(TState state);

        double PathCost(TState state, TAction action);
    }

    /// <summary>
    /// Search nodes are popped in minimum-priority order
    /// </summary>
    public class MinPriorityFrontier<TState, TAction> : ISearchFrontier<TState, TAction>
    {
        private readonly MinPriorityQueue<SearchNode<TState, TAction>> _queue;
        private readonly HashSet<TState> _states;

        public int Size => _states.Count;

        public MinPriorityFrontier(IComparer<SearchNode<TState, TAction>> nodeComparer)
        {
            _queue = new MinPriorityQueue<SearchNode<TState, TAction>>(nodeComparer);
            _states = new HashSet<TState>();
        }

        public void Push(SearchNode<TState, TAction> node)
        {
            _states.Add(node.State);
            _queue.Push(node);
        }

        public SearchNode<TState, TAction> Pop()
        {
            var node = _queue.Pop();
            _states.Remove(node.State);
            return node;
        }

        public bool ContainsState(TState state)
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

        public void Remove(T item)
        {
            if (Size == 0) throw new InvalidOperationException("cannot remove from empty");

            var idx = IndexOf(item, 0);

            if (idx == -1) throw new InvalidOperationException("cannot remove item not in heap");

            RemoveAtIdx(idx);
        }

        public bool Contains(T item)
        {
            return IndexOf(item, 0) >= 0;
        }

        public T RemoveMin()
        {
            if (Size == 0) throw new InvalidOperationException("cannot remove from empty");
            return RemoveAtIdx(0);
        }

        public T PeekMin()
        {
            if (Size == 0) throw new InvalidOperationException("cannot peek when empty");
            return _buf[0];
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

        private int IndexOf(T item, int subRoot)
        {
            if (subRoot >= Size)
            {
                // gone past leaf
                return -1;
            }
            if (_comparer.Compare(item, _buf[subRoot]) == -1)
            {
                // item is less than current node - will not be in this subtree
                return -1;
            }
            if (item.Equals(_buf[subRoot]))
            {
                return subRoot;
            }

            var idx = IndexOf(item, subRoot * 2 + 1);
            return idx >= 0
                ? idx
                : IndexOf(item, subRoot * 2 + 2);
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
            var temp = _buf[idxA];
            _buf[idxA] = _buf[idxB];
            _buf[idxB] = temp;
        }
    }
}
