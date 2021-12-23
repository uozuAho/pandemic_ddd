using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates;
using pandemic.drawing;
using pandemic.Values;

namespace pandemic.console
{
    internal class BfsRunner
    {
        public static void Run()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist }
            });
            var searchProblem = new PandemicSearchProblem(game, new PlayerCommandGeneratorFast());
            var searcher = new GreedyBestFirstSearch(searchProblem);

            Console.WriteLine("Searching...");
            var steps = 0;
            var sw = Stopwatch.StartNew();
            var bestState = new SearchNode(game, null, null, 234, int.MinValue);

            while (!searcher.IsFinished)
            {
                var state = searcher.Step();
                if (state != null && state.Score > bestState.Score)
                    bestState = state;
                steps++;

                if (sw.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"explored: {steps}. queued: {searcher.Frontier.Size}. Best state found:");
                    Console.WriteLine(NodeLabel(bestState.State));
                    sw.Restart();
                }
            }
        }

        public static void Draw(int numNodes)
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist }
            });
            var searchProblem = new PandemicSearchProblem(game, new PlayerCommandGeneratorFast());
            var searcher = new GreedyBestFirstSearch(searchProblem);

            Console.WriteLine("Searching...");
            var nodesSearched = new List<SearchNode>();

            for (var i = 0; i < numNodes && !searcher.IsFinished; i++)
            {
                var currentNode = searcher.Step();
                if (currentNode != null) nodesSearched.Add(currentNode);
            }

            var graph = ToDrawerGraph(nodesSearched);
            CsDotDrawer.FromGraph(graph).SaveToFile("bfs.dot");
        }

        private static DrawerGraph ToDrawerGraph(List<SearchNode> nodes)
        {
            var graph = new DrawerGraph();
            var visitedNodes = new Dictionary<SearchNode, DrawerNode>();

            foreach (var node in nodes)
            {
                var drawerNode = graph.CreateNode(NodeLabel(node.State));
                visitedNodes[node] = drawerNode;
                if (node.Parent != null && visitedNodes.ContainsKey(node.Parent))
                {
                    var parent = visitedNodes[node.Parent];
                    graph.CreateEdge(parent, drawerNode, node.Action?.ToString() ?? string.Empty);
                }
            }

            return graph;
        }

        private static string NodeLabel(PandemicGame state)
        {
            var nodeValue = GameEvaluator.Evaluate(state);
            var cured = string.Join(",", state.CureDiscovered.Where(c => c.Value).Select(c => c.Key));
            var players = string.Join("\\n", state.Players.Select(PlayerText));
            var researchStations = string.Join(",", state.Cities.Where(c => c.HasResearchStation)
                .Select(c => c.Name));

            var sb = new StringBuilder();
            sb.Append($"val: {nodeValue}\\n");
            if (cured.Length > 1) sb.Append($"Cured: {cured}\\n");
            sb.Append($"Stations: {researchStations}\\n");
            sb.Append($"{players}");

            return sb.ToString();
        }

        private static string PlayerText(Player player)
        {
            var counts = string.Join(",", player.Hand.CityCards
                .GroupBy(c => c.City.Colour)
                .Select(g => $"{g.Key.ToString().First()}:{g.Count()}"));

            return $"p:{counts}";
        }
    }
}