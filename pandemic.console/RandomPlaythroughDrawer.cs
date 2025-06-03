namespace pandemic.console;

using System;
using System.Linq;
using Aggregates.Game;
using Commands;
using drawing;
using utils;
using Values;
using Colour = drawing.Colour;

internal class RandomPlaythroughDrawer
{
    public static void DoIt()
    {
        var random = new Random();
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                Roles = [Role.Medic, Role.Scientist],
            }
        );

        var graph = new DrawerGraph();
        var prevNode = graph.CreateNode(game.CurrentPlayer.Role.ToString());
        var currentNode = prevNode;

        while (!game.IsOver)
        {
            var actions = PlayerCommandGenerator.AllLegalCommands(game).ToList();
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
            currentNode.Label = game.IsOver ? game.LossReason : game.CurrentPlayer.Role.ToString();
            prevNode = currentNode;
        }

        CsDotDrawer.FromGraph(graph).SaveToFile("asdf.dot");
    }

    private static DrawerNode AddSelectedAction(
        DrawerGraph graph,
        DrawerNode prevState,
        IPlayerCommand selectedAction
    )
    {
        var currentState = graph.CreateNode();
        currentState.Colour = Colour.Red;
        var edge = graph.CreateEdge(prevState, currentState, selectedAction.ToString());
        edge.Colour = Colour.Red;
        return currentState;
    }

    private static void AddUnselectedAction(
        DrawerGraph graph,
        DrawerNode prevState,
        IPlayerCommand action
    )
    {
        var currentState = graph.CreateNode();
        _ = graph.CreateEdge(prevState, currentState, action.ToString());
    }
}
