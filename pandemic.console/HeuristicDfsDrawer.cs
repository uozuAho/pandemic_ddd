using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.drawing;

namespace pandemic.console
{
    public class HeuristicDfsDrawer
    {
        private const int NodeLimit = 300;
        private static readonly Random _rng = new();
        private static readonly PlayerCommandGenerator CommandGenerator = new();

        public static void DrawSearch(PandemicGame game)
        {
            var root = new SearchNode(game, null, null);
            var tracker = new NodeTracker();

            Dfs(root, tracker);

            var graph = new DrawerGraph();
            var drawerRoot = graph.CreateNode();
            ExpandGraph(graph, root, drawerRoot);

            CsDotDrawer.FromGraph(graph).SaveToFile("hdfs.dot");
        }

        private static void Dfs(SearchNode node, NodeTracker nodeTracker)
        {
            if (nodeTracker.TotalNumNodes == NodeLimit) return;
            if (!DfsWithHeuristicsAgent.CanWin(node.State)) return;

            var legalActions = CommandGenerator.AllLegalCommands(node.State)
                .OrderBy(a => DfsWithHeuristicsAgent.CommandPriority(a, node.State))
                // shuffle, otherwise we're at the mercy of the order of the move generator
                .ThenBy(_ => _rng.Next()).ToList();

            foreach (var action in legalActions)
            {
                if (nodeTracker.TotalNumNodes == NodeLimit) return;

                var (childState, _) = node.State.Do(action);
                var child = new SearchNode(childState, action, node);
                node.Children.Add(child);
                nodeTracker.TotalNumNodes++;
                Dfs(child, nodeTracker);
            }
        }

        private static void ExpandGraph(DrawerGraph graph, SearchNode node, DrawerNode drawerNode)
        {
            if (node.State.IsLost)
            {
                drawerNode.Label = node.State.LossReason;
            }
            else if (!DfsWithHeuristicsAgent.CanWin(node.State))
            {
                drawerNode.Label = DfsWithHeuristicsAgent.ReasonGameCannotBeWon(node.State);
            }
            else
            {
                var game = node.State;
                var cures = string.Join("", game.CuresDiscovered.Select(c => c.Colour));
                drawerNode.Label = $"deck: {game.PlayerDrawPile.Count}. Cures: {cures}";
            }

            foreach (var child in node.Children)
            {
                var drawerChild = graph.CreateNode();
                graph.CreateEdge(drawerNode, drawerChild, child.Command!.ToString());
                ExpandGraph(graph, child, drawerChild);
            }
        }

        /// <summary>
        /// Action: command that resulted in State
        /// </summary>
        private record SearchNode(
            PandemicGame State,
            IPlayerCommand? Command,
            SearchNode? Parent
        )
        {
            public readonly List<SearchNode> Children = new();
        }

        private class NodeTracker
        {
            public int TotalNumNodes { get; set; } = 0;
        }
    }
}
