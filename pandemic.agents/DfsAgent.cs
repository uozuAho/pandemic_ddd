using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Commands;

namespace pandemic.agents
{
    /// <summary>
    /// Depth-first search through the game for a winning sequence of commands
    /// </summary>
    public class DfsAgent
    {
        private static readonly Random _rng = new();
        private static readonly PlayerCommandGenerator _commandGenerator = new();

        public IEnumerable<PlayerCommand> CommandsToWin(PandemicGame game)
        {
            var root = new SearchNode(game, null, null);

            var diagnostics = Diagnostics.StartNew();
            var win = Hunt(root, 0, diagnostics);
            if (win == null) return Enumerable.Empty<PlayerCommand>();

            var winningCommands = new List<PlayerCommand>();
            while (win.Parent != null)
            {
                if (win.Command == null) throw new InvalidOperationException("no!");

                winningCommands.Add(win.Command);

                win = win.Parent;
            }

            winningCommands.Reverse();

            return winningCommands;
        }

        private static SearchNode? Hunt(SearchNode node, int depth, Diagnostics diagnostics)
        {
            if (node.State.IsWon) return node;
            diagnostics.NodeExplored();
            diagnostics.Depth(depth);
            if (node.State.IsLost)
                diagnostics.Loss(node.State.LossReason);

            var legalActions = _commandGenerator.LegalCommands(node.State)
                // shuffle, otherwise we're at the mercy of the order of the move generator
                .OrderBy(_ => _rng.Next()).ToList();

            foreach (var action in legalActions)
            {
                var childState = node.State.Copy();
                childState.Do(action);
                var child = new SearchNode(childState, action, node);
                var winningNode = Hunt(child, depth + 1, diagnostics);
                if (winningNode != null)
                    return winningNode;
            }

            return null;
        }

        /// <summary>
        /// Action: command that resulted in State
        /// </summary>
        private record SearchNode(
            PandemicGame State,
            PlayerCommand? Command,
            SearchNode? Parent
        );

        private class Diagnostics
        {
            private readonly Stopwatch _stopwatch;
            private int _nodesExplored;
            private int _maxDepth;
            private readonly Dictionary<string, int> _losses = new();

            private Diagnostics(Stopwatch stopwatch)
            {
                _stopwatch = stopwatch;
            }

            public static Diagnostics StartNew()
            {
                return new Diagnostics(Stopwatch.StartNew());
            }

            public void NodeExplored()
            {
                _nodesExplored++;
                Report();
            }

            private void Report()
            {
                if (_stopwatch.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"nodes explored: {_nodesExplored}. Max depth {_maxDepth}. Losses: {string.Join(',', _losses)}");
                    _stopwatch.Restart();
                }
            }

            public void Depth(int depth)
            {
                if (depth > _maxDepth)
                    _maxDepth = depth;
            }

            public void Loss(string reason)
            {
                if (_losses.ContainsKey(reason))
                    _losses[reason]++;
                else
                    _losses[reason] = 1;
            }
        }
    }
}
