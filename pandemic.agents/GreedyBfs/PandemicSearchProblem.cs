using System;
using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Commands;

namespace pandemic.agents.GreedyBfs;

public class PandemicSearchProblem
{
    public PandemicGame InitialState { get; }

    private readonly PlayerCommandGenerator _commandGenerator;

    public PandemicSearchProblem(
        PandemicGame initialState,
        PlayerCommandGenerator commandGenerator)
    {
        InitialState = initialState;
        _commandGenerator = commandGenerator;
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
