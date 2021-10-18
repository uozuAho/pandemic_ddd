using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.agents
{
    /// <summary>
    /// Depth-first search, with hand-crafted command preferences
    /// </summary>
    public class DfsWithHeuristicsAgent : IPandemicGameSolver
    {
        private static readonly Random _rng = new Random();

        public IEnumerable<PlayerCommand> CommandsToWin(PandemicSpielGameState state)
        {
            var root = new SearchNode(state, null, null);

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
            if (node.State.IsWin) return node;
            diagnostics.NodeExplored();
            diagnostics.Depth(depth);
            if (node.State.IsLoss)
                diagnostics.Loss(node.State.Game.LossReason);

            var legalActions = node.State.LegalActions()
                .OrderBy(a => CommandPriority(a, node.State.Game))
                // shuffle, otherwise we're at the mercy of the order of the move generator
                .ThenBy(_ => _rng.Next()).ToList();

            foreach (var action in legalActions)
            {
                var childState = new PandemicSpielGameState(node.State.Game);
                childState.ApplyAction(action);
                var child = new SearchNode(childState, action, node);
                var winningNode = Hunt(child, depth + 1, diagnostics);
                if (winningNode != null)
                    return winningNode;
            }

            return null;
        }

        /// <summary>
        /// Lower number = higher priority. There's plenty more that could be done here:
        /// - prefer to move towards research stations
        /// - don't build research stations with cards that could be used to cure
        /// - players work together: aim to cure different diseases per player
        /// </summary>
        private static int CommandPriority(PlayerCommand command, PandemicGame game)
        {
            return command switch
            {
                DiscoverCureCommand => 0,
                BuildResearchStationCommand => 1,
                DriveFerryCommand => 2,
                DiscardPlayerCardCommand d => DiscardPriority(3, d, game),
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };
        }

        private static int DiscardPriority(int basePriority, DiscardPlayerCardCommand command, PandemicGame game)
        {
            // prefer to keep cards with matching colours. returns, for example:
            // -> [(blue, 1), (red, 2)]
            var handByNumberOfColoursAscending = game.CurrentPlayer.Hand.CityCards
                .GroupBy(c => c.City.Colour)
                .OrderBy(g => g.Count())
                .ToList();

            // todo: don't put epidemic cards in hand
            var cardToDiscard = command.Card as PlayerCityCard;
            if (cardToDiscard == null) return basePriority;

            return basePriority + handByNumberOfColoursAscending.FindIndex(c => c.Key == cardToDiscard.City.Colour);
        }

        /// <summary>
        /// Command: command that resulted in State
        /// </summary>
        private record SearchNode(
            PandemicSpielGameState State,
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
