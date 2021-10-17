using System;
using System.Collections.Generic;
using System.Linq;

namespace pandemic.agents
{
    public class DfsAgent
    {
        public IEnumerable<PlayerCommand> CommandsToWin(PandemicSpielGameState state)
        {
            var root = new SearchNode(state, null, null);

            var win = Hunt(root);
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

        private static SearchNode? Hunt(SearchNode node)
        {
            if (node.State.IsWin) return node;

            foreach (var action in node.State.LegalActions())
            {
                var childState = new PandemicSpielGameState(node.State.Game);
                childState.ApplyAction(action);
                var child = new SearchNode(childState, action, node);
                var winningNode = Hunt(child);
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
}
