using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Commands;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.agents
{
    /// <summary>
    /// Depth-first search, with hand-crafted command preferences
    /// </summary>
    public class DfsWithHeuristicsAgent : IPandemicGameSolver
    {
        private static readonly Random _rng = new Random();
        private static readonly PlayerCommandGenerator CommandGenerator = new();

        public IEnumerable<PlayerCommand> CommandsToWin(
            PandemicGame state,
            TimeSpan timeout)
        {
            var root = new SearchNode(state, null, null);

            var diagnostics = Diagnostics.StartNew();
            var stopwatch = Stopwatch.StartNew();
            var cardCounter = new CardCounter();
            var win = Hunt(root, 0, diagnostics, cardCounter, stopwatch, timeout);
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

        /// <summary>
        /// Returns true if it's possible to win from the given state (not checked exhaustively)
        /// </summary>
        public static bool CanWin(PandemicGame game, CardCounter? cardCounter = null)
        {
            return ReasonGameCannotBeWon(game, cardCounter) == string.Empty;
        }

        public static string ReasonGameCannotBeWon(PandemicGame game, CardCounter? cardCounter = null)
        {
            if (game.IsLost)
            {
                return $"game is lost: {game.LossReason}";
            }
            if (cardCounter != null)
            {
                if (!EnoughCardsLeftToCureAll(game, cardCounter, out var reason)) return reason;
            }
            else if (!EnoughCardsLeftToCureAll(game)) return "not enough cards left to cure";

            return string.Empty;
        }

        private static bool EnoughCardsLeftToCureAll(PandemicGame game, CardCounter cardCounter, out string reason)
        {
            foreach (var colour in ColourExtensions.AllColours)
            {
                // 5 cards needed to cure. Ignores role special abilities
                if (!game.CureDiscovered[colour] && cardCounter.CardsAvailable[colour] < 5)
                {
                    reason = $"Cannot cure {colour}";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Returns true if there are not enough cards to cure all diseases, regardless of card colours
        /// </summary>
        private static bool EnoughCardsLeftToCureAll(PandemicGame game)
        {
            var cardsNeededForAllCures = game.CureDiscovered.Sum(c => c.Value ? 0 : 5); // ignores special abilities
            var cardsAvailable = game.Players.Sum(p => p.Hand.CityCards.Count()) + game.PlayerDrawPile.Count;

            return cardsAvailable >= cardsNeededForAllCures;
        }

        /// <summary>
        /// Lower number = higher priority. There's plenty more that could be done here:
        /// - prefer to move towards research stations
        /// - don't build research stations with cards that could be used to cure
        /// - players work together: aim to cure different diseases per player
        /// </summary>
        public static int CommandPriority(PlayerCommand command, PandemicGame game)
        {
            return command switch
            {
                DiscoverCureCommand => 0,
                BuildResearchStationCommand => 10,
                DriveFerryCommand => 20,
                DiscardPlayerCardCommand d => DiscardPriority(30, d, game),
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };
        }

        private static SearchNode? Hunt(
            SearchNode node,
            int depth,
            Diagnostics diagnostics,
            CardCounter cardCounter,
            Stopwatch stopwatch,
            TimeSpan timeout)
        {
            diagnostics.NodeExplored();
            diagnostics.Depth(depth);

            if (stopwatch.Elapsed > timeout) throw new TimeoutException();

            if (node.State.IsWon) return node;
            if (!CanWin(node.State, cardCounter))
            {
                diagnostics.StoppedExploringBecause(ReasonGameCannotBeWon(node.State, cardCounter));
                return null;
            }

            var comparer = new CommandPriorityComparer(node.State);
            var legalActions = CommandGenerator.LegalCommands(node.State)
                // .OrderBy(a => CommandPriority(a, node.State.Game))
                .OrderBy(a => a, comparer)
                // shuffle, otherwise we're at the mercy of the order of the move generator
                .ThenBy(_ => _rng.Next()).ToList();

            foreach (var action in legalActions)
            {
                var childCardCounter = cardCounter.Clone();
                var (childState, events) = node.State.Do(action);
                foreach (var @event in events.OfType<PlayerCardDiscarded>())
                {
                    if (@event.Card is PlayerCityCard cityCard)
                        childCardCounter.CardsAvailable[cityCard.City.Colour]--;
                }
                var child = new SearchNode(childState, action, node);
                var winningNode = Hunt(child, depth + 1, diagnostics, childCardCounter, stopwatch, timeout);
                if (winningNode != null)
                    return winningNode;
            }

            return null;
        }

        private static int DiscardPriority(int basePriority, DiscardPlayerCardCommand command, PandemicGame game)
        {
            // prefer to keep cards with matching colours. returns, for example:
            // -> [(blue, 1), (red, 2)]
            var handByNumberOfColoursAscending = game.CurrentPlayer.Hand.CityCards
                .GroupBy(c => c.City.Colour)
                .OrderBy(g => g.Count())
                .ToList();

            if (command.Card is not PlayerCityCard cardToDiscard) return basePriority;

            return basePriority + handByNumberOfColoursAscending.FindIndex(c => c.Key == cardToDiscard.City.Colour);
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
            private readonly Dictionary<string, int> _stopReasons = new();

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
                    Console.WriteLine($"nodes explored: {_nodesExplored}. Stops: \n  {string.Join("\n  ", _stopReasons)}");
                    _stopwatch.Restart();
                }
            }

            public void Depth(int depth)
            {
                if (depth > _maxDepth)
                    _maxDepth = depth;
            }

            public void StoppedExploringBecause(string reason)
            {
                if (_stopReasons.ContainsKey(reason))
                    _stopReasons[reason]++;
                else
                    _stopReasons[reason] = 1;
            }
        }
    }
}
