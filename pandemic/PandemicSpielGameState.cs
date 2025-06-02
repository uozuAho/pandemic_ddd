namespace pandemic;

using System;
using System.Collections.Generic;
using System.Linq;
using Aggregates.Game;
using Commands;
using Events;

/// <summary>
/// Provides an OpenSpiel compatible interface for playing Pandemic.
/// Work in progress. See todos.
/// </summary>
public class PandemicSpielGameState(PandemicGame game)
{
    public PandemicGame Game = game;

    public bool IsTerminal => Game.IsOver;
    public bool IsWin => Game.IsWon;
    public bool IsLoss => Game.IsLost;
    public int CurrentPlayerIdx => Game.CurrentPlayerIdx;

    public double[] Returns =>
        IsWin ? Enumerable.Repeat(1.0, Game.Players.Length).ToArray()
        : IsLoss ? Enumerable.Repeat(-1.0, Game.Players.Length).ToArray()
        : Enumerable.Repeat(0.0, Game.Players.Length).ToArray();

    private readonly PlayerCommandGenerator _commandGenerator = new();

    public PandemicSpielGameState Clone()
    {
        return new PandemicSpielGameState(Game.Copy());
    }

    public override string ToString()
    {
        return Game.ToString();
    }

    public IEnumerable<IPlayerCommand> LegalActions()
    {
        return _commandGenerator.AllLegalCommands(Game);
    }

    public void ApplyActionInt(int action)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEvent> ApplyAction(int action)
    {
        var legalActions = _commandGenerator.AllLegalCommands(Game).ToList();
        return ApplyAction(legalActions[action]);
    }

    public IEnumerable<IEvent> ApplyAction(IPlayerCommand action)
    {
        var (game, events) = Game.Do(action);
        Game = game;
        return events;
    }
}
