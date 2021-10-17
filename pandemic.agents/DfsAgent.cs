using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace pandemic.agents
{
    public class DfsAgent
    {
        public IEnumerable<PlayerCommand> CommandsToWin(PandemicSpielGameState state)
        {
            var root = new SearchNode(state, null, null);

            var diagnostics = Diagnostics.StartNew();
            var win = Hunt(root, diagnostics);
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

        private static SearchNode? Hunt(SearchNode node, Diagnostics diagnostics)
        {
            if (node.State.IsWin) return node;
            diagnostics.NodeExplored();

            foreach (var action in node.State.LegalActions())
            {
                var childState = new PandemicSpielGameState(node.State.Game);
                childState.ApplyAction(action);
                var child = new SearchNode(childState, action, node);
                var winningNode = Hunt(child, diagnostics);
                if (winningNode != null)
                    return winningNode;
            }

            return null;
        }
    }

    /// <summary>
    /// Command: command that resulted in State
    /// </summary>
    internal record SearchNode(
        PandemicSpielGameState State,
        PlayerCommand? Command,
        SearchNode? Parent
    );

    internal class Diagnostics
    {
        private readonly Stopwatch _stopwatch;

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
            Report();
        }

        private void Report()
        {
            if (_stopwatch.ElapsedMilliseconds > 1000)
            {
                Console.WriteLine("yo");
                _stopwatch.Restart();
            }
        }
    }
}
