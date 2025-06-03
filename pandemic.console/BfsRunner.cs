namespace pandemic.console;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using agents.GameEvaluators;
using agents.GreedyBfs;
using Aggregates.Game;
using drawing;
using utils;
using Values;
using SearchNode = agents.GreedyBfs.SearchNode;

internal class BfsRunner
{
    public static void Run()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist },
            }
        );
        var searcher = new GreedyBestFirstSearch(game);

        Console.WriteLine("Searching...");
        var steps = 0;
        var sw = Stopwatch.StartNew();
        var bestState = new SearchNode(game, null, null, 234, int.MinValue);

        while (!searcher.IsFinished)
        {
            var state = searcher.Step();
            if (state != null && state.Score > bestState.Score)
            {
                bestState = state;
            }

            steps++;

            if (sw.ElapsedMilliseconds > 2000)
            {
                Console.WriteLine(
                    $"explored: {steps}. queued: {searcher.Frontier.Size}. Best state found:"
                );
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
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist },
            }
        );
        var searcher = new GreedyBestFirstSearch(game);

        Console.WriteLine("Searching...");
        var nodesSearched = new List<SearchNode>();

        for (var i = 0; i < numNodes && !searcher.IsFinished; i++)
        {
            var currentNode = searcher.Step();
            if (currentNode != null)
            {
                nodesSearched.Add(currentNode);
            }
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
            if (node.Parent != null && visitedNodes.TryGetValue(node.Parent, out var value))
            {
                _ = graph.CreateEdge(value, drawerNode, node.Action?.ToString() ?? string.Empty);
            }
        }

        return graph;
    }

    private static string NodeLabel(PandemicGame state)
    {
        var nodeValue = HandCraftedGameEvaluator.Score(state);
        var cured = string.Join(",", state.CuresDiscovered.Select(c => c.Colour));
        var players = string.Join("\\n", state.Players.Select(PlayerText));
        var researchStations = string.Join(
            ",",
            state.Cities.Where(c => c.HasResearchStation).Select(c => c.Name)
        );

        var sb = new StringBuilder();
        _ = sb.Append($"val: {nodeValue}\\n");
        if (cured.Length > 1)
        {
            _ = sb.Append($"Cured: {cured}\\n");
        }

        _ = sb.Append($"Stations: {researchStations}\\n");
        _ = sb.Append($"{players}");

        return sb.ToString();
    }

    private static string PlayerText(Player player)
    {
        var counts = string.Join(
            ",",
            player
                .Hand.CityCards()
                .GroupBy(c => c.City.Colour)
                .Select(g => $"{g.Key.ToString().First()}:{g.Count()}")
        );

        return $"p:{counts}";
    }
}
