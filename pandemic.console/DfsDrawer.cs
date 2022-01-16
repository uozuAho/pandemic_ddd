using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Commands;
using pandemic.drawing;

namespace pandemic.console
{
    public class DfsDrawer
    {
        private const int NodeLimit = 300;
        private static readonly Random _rng = new();

        public static void DrawSearch(PandemicGame game)
        {
            var state = new PandemicSpielGameState(game);
            var root = new SearchNode(state, null, null);
            var tracker = new NodeTracker();

            Dfs(root, tracker);

            var graph = new DrawerGraph();
            var drawerRoot = graph.CreateNode();
            ExpandGraph(graph, root, drawerRoot);

            CsDotDrawer.FromGraph(graph).SaveToFile("dfs.dot");
        }

        private static void Dfs(SearchNode node, NodeTracker nodeTracker)
        {
            if (nodeTracker.TotalNumNodes == NodeLimit) return;

            var legalActions = node.State.LegalActions()
                // shuffle, otherwise we're at the mercy of the order of the move generator
                .OrderBy(_ => _rng.Next()).ToList();

            foreach (var action in legalActions)
            {
                if (nodeTracker.TotalNumNodes == NodeLimit) return;

                var childState = new PandemicSpielGameState(node.State.Game);
                childState.ApplyAction(action);
                var child = new SearchNode(childState, action, node);
                node.Children.Add(child);
                nodeTracker.TotalNumNodes++;
                Dfs(child, nodeTracker);
            }
        }

        private static void ExpandGraph(DrawerGraph graph, SearchNode node, DrawerNode drawerNode)
        {
            foreach (var child in node.Children)
            {
                var drawerChild = graph.CreateNode();
                if (child.State.IsLoss)
                    drawerChild.Label = child.State.Game.LossReason;
                graph.CreateEdge(drawerNode, drawerChild, child.Command!.ToString());
                ExpandGraph(graph, child, drawerChild);
            }
        }

        /// <summary>
        /// Action: command that resulted in State
        /// </summary>
        private record SearchNode(
            PandemicSpielGameState State,
            PlayerCommand? Command,
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
