using System;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using utils;

namespace pandemic.agents;

public class RandomAgent : ILiveAgent
{
    public IPlayerCommand NextCommand(PandemicGame game)
    {
        var random = new Random();
        return random.Choice(game.LegalCommands());
    }
}
