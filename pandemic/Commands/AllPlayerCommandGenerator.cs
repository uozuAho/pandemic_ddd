using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.Commands;

public static class AllPlayerCommandGenerator
{
    /// <summary>
    /// Not really _all_ possible commands, but a lot
    /// </summary>
    public static IEnumerable<IPlayerCommand> AllPossibleCommands(PandemicGame game)
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
                foreach (var colour in ColourExtensions.AllColours)
                {
                    yield return new TreatDiseaseCommand(player.Role, city.Name, colour);
                }

            }
        }

        foreach (var player in game.Players)
        {
            yield return new PassCommand(player.Role);

            foreach (var cardsToCure in player.Hand.CityCards
                         .GroupBy(c => c.City.Colour)
                         .Where(g => g.Count() >= 5))
            {
                yield return new DiscoverCureCommand(player.Role, cardsToCure.Select(g => g).ToArray());
            }

            foreach (var card in player.Hand.CityCards)
            {
                foreach (var otherPlayer in game.Players)
                {
                    yield return new ShareKnowledgeGiveCommand(player.Role, card.City.Name, otherPlayer.Role);
                    yield return new ShareKnowledgeTakeCommand(player.Role, card.City.Name, otherPlayer.Role);
                }
            }

            foreach (var city in game.Cities)
            {
                yield return new GovernmentGrantCommand(player.Role, city.Name);

                foreach (var player2 in game.Players)
                {
                    yield return new AirliftCommand(player.Role, player2.Role, city.Name);
                    yield return new DispatcherDriveFerryPawnCommand(player2.Role, city.Name);
                }
            }

            yield return new EventForecastCommand(player.Role, game.InfectionDiscardPile.Top(6).ToImmutableList());

            foreach (var card in game.InfectionDiscardPile.Cards)
            {
                yield return new ResilientPopulationCommand(player.Role, card);
            }

            yield return new OneQuietNightCommand(player.Role);

            foreach (var player2 in game.Players)
            {
                yield return new DispatcherMovePawnToOtherPawnCommand(player.Role, player2.Role);
            }
        }
    }
}
