using System.Collections.Generic;
using pandemic.Aggregates.Game;
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
        var (nextState, _) = state.Do(action);

        return nextState;
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
