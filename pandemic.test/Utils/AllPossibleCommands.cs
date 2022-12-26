using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.GameData;

namespace pandemic.test.Utils;

public class AllPossibleCommands
{
    public static IEnumerable<PlayerCommand> GenerateAllPossibleCommands(PandemicGame game)
    {
        foreach (var city in game.Cities)
        {
            yield return new DiscardPlayerCardCommand(PlayerCards.CityCard(city.Name));
            foreach (var player in game.Players)
            {
                yield return new BuildResearchStationCommand(player.Role, city.Name);
                yield return new DriveFerryCommand(player.Role, city.Name);
                yield return new DirectFlightCommand(player.Role, city.Name);
                yield return new CharterFlightCommand(player.Role, PlayerCards.CityCard(player.Location), city.Name);
                yield return new ShuttleFlightCommand(player.Role, city.Name);
            }
        }
    }
}
