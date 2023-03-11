using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using pandemic.agents;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates.Game;
using pandemic.drawing;
using pandemic.Values;
using utils;
using SearchNode = pandemic.agents.GreedyBfs.SearchNode;

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

                if (sw.ElapsedMilliseconds > 2000)
                {
                    Console.WriteLine($"explored: {steps}. queued: {searcher.Frontier.Size}. Best state found:");
                    Console.WriteLine(PandemicGameStringRenderer.ShortState(bestState.State));
                    sw.Restart();
                    break;
                }
            }

            if (searcher.IsSolved)
            {
                Console.WriteLine("solved!");
                return;
            }

            var states = new List<SearchNode>();
            while (bestState.Parent != null)
            {
                states.Add(bestState);
                bestState = bestState.Parent;
            }

            Console.WriteLine("path to best state (last 20):");
            foreach (var state in states.Reversed().TakeLast(20))
            {
                Console.WriteLine(state.Action);
                Console.WriteLine(PandemicGameStringRenderer.ShortState(state.State));
            }

            // Console.WriteLine("Solution:");
            // var currentState = game;
            // var asdf = new PandemicSearchProblem(game, new PlayerCommandGenerator());
            // var commands = searcher.GetSolution();
            // foreach (var command in commands)
            // {
            //     Console.WriteLine(command.ToString());
            //     currentState = asdf.DoAction(currentState, command);
            // }
            // Console.WriteLine();
            // Console.WriteLine("Final state:");
            // Console.WriteLine(currentState);
        }

        public static void Draw(int numNodes)
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist }
            });
            var searchProblem = new PandemicSearchProblem(game);
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
            var nodeValue = GameEvaluator.Score(state);
            var cured = string.Join(",", state.CuresDiscovered.Select(c => c.Colour));
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
