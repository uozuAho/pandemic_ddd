using System;
using System.Collections.Generic;
using pandemic.Aggregates;

namespace pandemic.agents.GreedyBfs;

public class PandemicSearchProblem : ISearchProblem<PandemicGame, PlayerCommand>
{
    public PandemicGame InitialState { get; }

    private readonly PlayerCommandGenerator _commandGenerator;

    public PandemicSearchProblem(PandemicGame initialState)
    {
        InitialState = initialState;
        _commandGenerator = new PlayerCommandGenerator();
    }

    public IEnumerable<PlayerCommand> GetActions(PandemicGame state)
    {
        return _commandGenerator.LegalCommands(state);
    }

    public PandemicGame DoAction(PandemicGame state, PlayerCommand action)
    {
        PandemicGame newState;

        switch (action)
        {
            case DriveFerryCommand command:
                (newState, _) = state.DriveOrFerryPlayer(command.Role, command.City);
                return newState;
            case DiscardPlayerCardCommand command:
                (newState, _) = state.DiscardPlayerCard(command.Card);
                return newState;
            case BuildResearchStationCommand command:
                (newState, _) = state.BuildResearchStation(command.City);
                return newState;
            case DiscoverCureCommand command:
                (newState, _) = state.DiscoverCure(command.Cards);
                return newState;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
        }
    }

    public bool IsGoal(PandemicGame state)
    {
        return state.IsWon;
    }

    public double PathCost(PandemicGame state, PlayerCommand action)
    {
        return 0.0;
    }
}
