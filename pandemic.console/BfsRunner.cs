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
            var searchProblem = new PandemicSearchProblem(game);
            var searcher = new GreedyBestFirstSearch<PandemicGame, PlayerCommand>(
                searchProblem, state => -GameEvaluator.Evaluate(state));

            Console.WriteLine("Searching...");
            var steps = 0;
            var sw = Stopwatch.StartNew();

            while (!searcher.IsFinished)
            {
                searcher.Step();
                steps++;

                if (sw.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"explored: {steps}. queued: {searcher.Frontier.Size}");
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
            var searchProblem = new PandemicSearchProblem(game);
            var searcher = new GreedyBestFirstSearch<PandemicGame, PlayerCommand>(
                searchProblem, state => -GameEvaluator.Evaluate(state));

            Console.WriteLine("Searching...");
            var nodesSearched = new List<SearchNode<PandemicGame, PlayerCommand>>();

            for (var i = 0; i < numNodes && !searcher.IsFinished; i++)
            {
                var currentNode = searcher.Step();
                if (currentNode != null) nodesSearched.Add(currentNode);
            }

            var graph = ToDrawerGraph(nodesSearched);
            CsDotDrawer.FromGraph(graph).SaveToFile("bfs.dot");
        }

        private static DrawerGraph ToDrawerGraph(List<SearchNode<PandemicGame, PlayerCommand>> nodes)
        {
            var graph = new DrawerGraph();
            var visitedNodes = new Dictionary<SearchNode<PandemicGame, PlayerCommand>, DrawerNode>();

            foreach (var node in nodes)
            {
                var drawerNode = graph.CreateNode(NodeLabel(node.State));
                visitedNodes[node] = drawerNode;
                if (node.Parent != null && visitedNodes.ContainsKey(node.Parent))
                {
                    var parent = visitedNodes[node.Parent];
                    graph.CreateEdge(parent, drawerNode, node.Action.ToString());
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
