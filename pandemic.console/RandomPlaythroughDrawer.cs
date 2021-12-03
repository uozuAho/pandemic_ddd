using System;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates;
using pandemic.drawing;
using pandemic.Values;
using utils;
using Colour = pandemic.drawing.Colour;

namespace pandemic.console;

class RandomPlaythroughDrawer
{
    public static void DoIt()
    {
        var random = new Random();
        var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
        {
            Difficulty = Difficulty.Introductory,
            Roles = new[] {Role.Medic, Role.Scientist}
        });
        var state = new PandemicSpielGameState(game);

        var graph = new DrawerGraph();
        var prevNode = graph.CreateNode(state.CurrentPlayer.Role.ToString());
        var currentNode = prevNode;

        while (!state.IsTerminal)
        {
            var actions = state.LegalActions().ToList();
            var selectedAction = random.Choice(actions);

            foreach (var action in actions)
            {
                if (action == selectedAction)
                {
                    currentNode = AddSelectedAction(graph, prevNode, action);
                }
                else
                {
                    AddUnselectedAction(graph, prevNode, action);
                }
            }

            state.ApplyAction(selectedAction);
            currentNode.Label = state.IsTerminal
                ? state.Game.LossReason
                : state.CurrentPlayer.Role.ToString();
            prevNode = currentNode;
        }

        CsDotDrawer.FromGraph(graph).SaveToFile("asdf.dot");
    }

    private static DrawerNode AddSelectedAction(DrawerGraph graph, DrawerNode prevState, PlayerCommand selectedAction)
    {
        var currentState = graph.CreateNode();
        currentState.Colour = Colour.Red;
        var edge = graph.CreateEdge(prevState, currentState, selectedAction.ToString());
        edge.Colour = Colour.Red;
        return currentState;
    }

    private static void AddUnselectedAction(DrawerGraph graph, DrawerNode prevState, PlayerCommand action)
    {
        var currentState = graph.CreateNode();
        graph.CreateEdge(prevState, currentState, action.ToString());
    }
}
