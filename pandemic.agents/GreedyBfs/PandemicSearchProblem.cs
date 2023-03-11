using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;

namespace pandemic.agents.GreedyBfs;

public class PandemicSearchProblem
{
    public PandemicGame InitialState { get; }

    public PandemicSearchProblem(PandemicGame initialState)
    {
        InitialState = initialState;
    }

    public IEnumerable<IPlayerCommand> GetActions(PandemicGame state)
    {
        return state.LegalCommands();
    }

    public PandemicGame DoAction(PandemicGame state, IPlayerCommand action)
    {
        var (nextState, _) = state.Do(action);

        return nextState;
    }

    public bool IsGoal(PandemicGame state)
    {
        return state.IsWon;
    }

    public double PathCost(PandemicGame state, IPlayerCommand action)
    {
        return 0.0;
    }
}
