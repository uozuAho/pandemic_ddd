namespace pandemic.agents;

using System;
using Aggregates.Game;
using Commands;
using utils;

public class RandomAgent : ILiveAgent
{
    public IPlayerCommand NextCommand(PandemicGame game)
    {
        var random = new Random();
        return random.Choice(game.LegalCommands());
    }
}
