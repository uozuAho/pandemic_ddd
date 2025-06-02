namespace pandemic.agents.GreedyBfs;

using System;
using System.Collections.Generic;
using Aggregates.Game;
using Commands;

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
public class GreedyBestFirstSearch
{
    public bool IsFinished { get; private set; }
    public bool IsSolved { get; private set; }
    public PandemicGame CurrentState { get; private set; }

    public MinPriorityFrontier Frontier;

    private readonly Dictionary<PandemicGame, SearchNode> _explored;

    public GreedyBestFirstSearch(PandemicGame game)
    {
        CurrentState = game;
        _explored = [];

        IsFinished = false;

        var nodeComparer = new SearchNodeComparer(CompareStates);
        Frontier = new MinPriorityFrontier(nodeComparer);
        var root = new SearchNode(game, null, default, 0, GameEvaluator.GameEvaluator.Score(game));
        Frontier.Push(root);
    }

    public SearchNode? Step()
    {
        if (IsFinished)
        {
            return null;
        }

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
        var commands = CurrentState.LegalCommands();
        foreach (var command in commands)
        {
            var (childState, _) = CurrentState.Do(command);
            var childCost = node.PathCost;
            var child = new SearchNode(
                childState,
                node,
                command,
                childCost,
                GameEvaluator.GameEvaluator.Score(childState)
            );

            if (_explored.ContainsKey(childState) || Frontier.ContainsState(childState))
            {
                continue;
            }

            if (CurrentState.IsWon)
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

    public IEnumerable<IPlayerCommand> GetSolution()
    {
        if (!IsSolved)
        {
            throw new InvalidOperationException("not solved!");
        }

        var actions = new List<IPlayerCommand>();
        var currentNode = _explored[CurrentState];
        while (currentNode is { Action: { } })
        {
            actions.Add(currentNode.Action);
            currentNode = currentNode.Parent;
        }

        actions.Reverse();
        return actions;
    }

    protected class SearchNodeComparer(Func<SearchNode, SearchNode, int> compare)
        : IComparer<SearchNode>
    {
        private readonly Func<SearchNode, SearchNode, int> _compare = compare;

        public int Compare(SearchNode? x, SearchNode? y)
        {
            if (x == null)
            {
                throw new NullReferenceException(nameof(x));
            }

            if (y == null)
            {
                throw new NullReferenceException(nameof(y));
            }

            return _compare(x, y);
        }
    }

    protected int CompareStates(SearchNode a, SearchNode b)
    {
        var priorityA = (double)-a.Score;
        var priorityB = (double)-b.Score;
        return priorityA < priorityB ? -1
            : priorityA > priorityB ? 1
            : 0;
    }
}

/// <summary>
/// </summary>
/// <param name="State"></param>
/// <param name="Parent"></param>
/// <param name="Action">Action that resulted in this state. Arbitrary value if parent is null</param>
/// <param name="PathCost">Path cost at this node = parent.path_cost + step_cost(parent, action)</param>
/// <param name="Score"></param>
public record SearchNode(
    PandemicGame State,
    SearchNode? Parent,
    IPlayerCommand? Action,
    double PathCost,
    int Score
);

/// <summary>
/// Search nodes are popped in minimum-priority order
/// </summary>
public class MinPriorityFrontier(IComparer<SearchNode> nodeComparer)
{
    private readonly MinPriorityQueue<SearchNode> _queue = new(nodeComparer);
    private readonly HashSet<PandemicGame> _states = [];

    public int Size => _states.Count;

    public void Push(SearchNode node)
    {
        _ = _states.Add(node.State);
        _queue.Push(node);
    }

    public SearchNode Pop()
    {
        var node = _queue.Pop();
        _ = _states.Remove(node.State);
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

public class MinPriorityQueue<T>(IComparer<T> comparer)
{
    private readonly BinaryMinHeap<T> _minHeap = new(comparer);

    public void Push(T item)
    {
        _minHeap.Add(item);
    }

    public T Pop()
    {
        return _minHeap.RemoveMin();
    }
}

public class BinaryMinHeap<T>(IComparer<T> comparer)
{
    public int Size => _buf.Count;

    private readonly List<T> _buf = [];
    private readonly IComparer<T> _comparer = comparer;

    public void Add(T item)
    {
        _buf.Add(item);
        Swim(Size - 1);
    }

    public T RemoveMin()
    {
        if (Size == 0)
        {
            throw new InvalidOperationException("cannot remove from empty");
        }

        return RemoveAtIdx(0);
    }

    private T RemoveAtIdx(int idx)
    {
        if (idx >= Size)
        {
            throw new ArgumentOutOfRangeException();
        }

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
        if (idx == 0)
        {
            return;
        }

        var parentIdx = (idx - 1) / 2;

        while (_comparer.Compare(_buf[idx], _buf[parentIdx]) == -1)
        {
            // swap if child < parent
            Swap(idx, parentIdx);
            if (parentIdx == 0)
            {
                return;
            }

            idx = parentIdx;
            parentIdx = (idx - 1) / 2;
        }
    }

    private void Sink(int idx)
    {
        while (true)
        {
            var leftIdx = (2 * idx) + 1;
            var rightIdx = (2 * idx) + 2;
            // stop if no children
            if (leftIdx >= Size)
            {
                return;
            }
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
