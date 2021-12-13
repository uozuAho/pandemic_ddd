using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates;
using pandemic.drawing;

namespace pandemic.console
{
    public class HeuristicDfsDrawer
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

            CsDotDrawer.FromGraph(graph).SaveToFile("hdfs.dot");
        }

        private static void Dfs(SearchNode node, NodeTracker nodeTracker)
        {
            if (nodeTracker.TotalNumNodes == NodeLimit) return;
            if (!DfsWithHeuristicsAgent.CanWin(node.State.Game)) return;

            var legalActions = node.State.LegalActions()
                .OrderBy(a => DfsWithHeuristicsAgent.CommandPriority(a, node.State.Game))
                // shuffle, otherwise we're at the mercy of the order of the move generator
                .ThenBy(_ => _rng.Next()).ToList();

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
            if (node.State.IsLoss)
            {
                drawerNode.Label = node.State.Game.LossReason;
            }
            else if (!DfsWithHeuristicsAgent.CanWin(node.State.Game))
            {
                drawerNode.Label = DfsWithHeuristicsAgent.ReasonGameCannotBeWon(node.State.Game);
            }
            else
            {
                var game = node.State.Game;
                var cures = string.Join("", game.CureDiscovered.Select(c => c.Value ? $"{c.Key} " : ""));
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
