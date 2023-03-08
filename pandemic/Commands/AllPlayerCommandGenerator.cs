using System;
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
        foreach (var cmd in StandardCommands(game)) yield return cmd;
        foreach (var cmd2 in SpecialEvents(game)) yield return cmd2;
        foreach (var cmd3 in RoleSpecialAbilities(game)) yield return cmd3;
    }

    private static IEnumerable<IPlayerCommand> SpecialEvents(PandemicGame game)
    {
        foreach (var player in game.Players)
        {
            foreach (var city in game.Cities)
            {
                yield return new GovernmentGrantCommand(player.Role, city.Name);

                foreach (var player2 in game.Players)
                {
                    yield return new AirliftCommand(player.Role, player2.Role, city.Name);
                }
            }

            yield return new EventForecastCommand(player.Role, game.InfectionDiscardPile.Top(6).ToImmutableList());

            foreach (var card in game.InfectionDiscardPile.Cards)
            {
                yield return new ResilientPopulationCommand(player.Role, card);
            }

            yield return new OneQuietNightCommand(player.Role);
        }
    }

    private static IEnumerable<IPlayerCommand> RoleSpecialAbilities(PandemicGame game)
    {
        foreach (var cmd in DispatcherCommands(game)) yield return cmd;
        foreach (var cmd in OperationsExpertCommands(game)) yield return cmd;
        foreach (var cmd in ResearcherCommands(game)) yield return cmd;
        foreach (var cmd in ScientistCommands(game)) yield return cmd;
        foreach (var cmd in ContingencyPlannerCommands(game)) yield return cmd;
    }

    private static IEnumerable<IPlayerCommand> ContingencyPlannerCommands(PandemicGame game)
    {
        if (!game.Players.Any(p => p.Role == Role.ContingencyPlanner)) yield break;

        foreach (var card in game.PlayerDiscardPile.Cards.Where(c => c is ISpecialEventCard).Cast<ISpecialEventCard>())
        {
            yield return new ContingencyPlannerTakeEventCardCommand(card);
        }
        // use of special event card is handled by SpecialEvents
    }

    private static IEnumerable<IPlayerCommand> ScientistCommands(PandemicGame game)
    {
        if (!game.Players.Any(p => p.Role == Role.Scientist)) yield break;

        var scientist = game.PlayerByRole(Role.Scientist);

        foreach (var cardsToCure in scientist.Hand.CityCards
                     .GroupBy(c => c.City.Colour)
                     .Where(g => g.Count() >= 4))
        {
            yield return new ScientistDiscoverCureCommand(cardsToCure.Select(g => g).ToArray());
        }
    }

    private static IEnumerable<IPlayerCommand> ResearcherCommands(PandemicGame game)
    {
        if (!game.Players.Any(p => p.Role == Role.Researcher)) yield break;

        foreach (var player in game.Players)
        {
            foreach (var city in game.Cities)
            {
                yield return new ResearcherShareKnowledgeGiveCommand(player.Role, city.Name);
                yield return new ShareKnowledgeTakeFromResearcherCommand(player.Role, city.Name);
            }
        }
    }

    private static IEnumerable<IPlayerCommand> OperationsExpertCommands(PandemicGame game)
    {
        if (!game.Players.Any(p => p.Role == Role.OperationsExpert)) yield break;

        yield return new OperationsExpertBuildResearchStation();
        foreach (var card in PlayerCards.CityCards)
        {
            foreach (var destination in game.Cities)
            {
                yield return new OperationsExpertDiscardToMoveFromStation(card, destination.Name);
            }
        }
    }

    private static IEnumerable<IPlayerCommand> DispatcherCommands(PandemicGame game)
    {
        if (!game.Players.Any(p => p.Role == Role.Dispatcher)) yield break;

        foreach (var player in game.Players)
        {
            foreach (var city in game.Cities)
            {
                foreach (var player2 in game.Players)
                {
                    yield return new DispatcherDriveFerryPawnCommand(player2.Role, city.Name);
                    yield return new DispatcherDirectFlyPawnCommand(player2.Role, city.Name);
                    yield return new DispatcherCharterFlyPawnCommand(player2.Role, city.Name);
                    yield return new DispatcherShuttleFlyPawnCommand(player2.Role, city.Name);
                }
            }

            foreach (var player2 in game.Players)
            {
                yield return new DispatcherMovePawnToOtherPawnCommand(player.Role, player2.Role);
            }
        }
    }

    private static IEnumerable<IPlayerCommand> StandardCommands(PandemicGame game)
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
        }
    }
}
