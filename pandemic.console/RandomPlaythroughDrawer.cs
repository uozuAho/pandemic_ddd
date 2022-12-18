using System;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.drawing;
using pandemic.Values;
using utils;
using Colour = pandemic.drawing.Colour;

namespace pandemic.console;

class RandomPlaythroughDrawer
{
    public static void DoIt()
    {
        var commandGenerator = new PlayerCommandGenerator();
        var random = new Random();
        var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
        {
            Difficulty = Difficulty.Introductory,
            Roles = new[] {Role.Medic, Role.Scientist}
        });

        var graph = new DrawerGraph();
        var prevNode = graph.CreateNode(game.CurrentPlayer.Role.ToString());
        var currentNode = prevNode;

        while (!game.IsOver)
        {
            var actions = commandGenerator.LegalCommands(game).ToList();
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

            var (updatedGame, _) = game.Do(selectedAction);
            game = updatedGame;
            currentNode.Label = game.IsOver
                ? game.LossReason
                : game.CurrentPlayer.Role.ToString();
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
