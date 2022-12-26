﻿using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.GameData;

namespace pandemic.test.Utils;

public class AllPossibleCommands
{
    public static IEnumerable<IPlayerCommand> GenerateAllPossibleCommands(PandemicGame game)
    {
        foreach (var city in game.Cities)
        {
            foreach (var player in game.Players)
            {
                yield return new DiscardPlayerCardCommand(player.Role, PlayerCards.CityCard(city.Name));
                yield return new BuildResearchStationCommand(player.Role, city.Name);
                yield return new DriveFerryCommand(player.Role, city.Name);
                yield return new DirectFlightCommand(player.Role, city.Name);
                yield return new CharterFlightCommand(player.Role, PlayerCards.CityCard(player.Location), city.Name);
                yield return new ShuttleFlightCommand(player.Role, city.Name);
            }
        }
    }
}
