using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic
{
    public class PlayerCommandGenerator
    {
        /// <summary>
        /// Determines available 'player commands' from the given game state.
        /// Player commands are any game action that requires a player to do
        /// something, eg:
        /// - performing an action by their role
        /// - playing an event card
        /// - drawing player cards
        /// </summary>
        public IEnumerable<PlayerCommand> LegalCommands(PandemicGame game)
        {
            if (game.IsOver) return Enumerable.Empty<PlayerCommand>();

            var discards = DiscardCommands(game).ToList();
            if (discards.Any())
                return discards;

            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                return DriveFerryCommands(game)
                    .Concat(BuildResearchStationCommands(game))
                    .Concat(CureCommands(game));
            }

            return Enumerable.Empty<PlayerCommand>();
        }

        private static IEnumerable<PlayerCommand> DiscardCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Hand.Count > 7)
            {
                foreach (var card in game.CurrentPlayer.Hand)
                {
                    yield return new DiscardPlayerCardCommand(card);
                }
            }
        }

        private static IEnumerable<PlayerCommand> DriveFerryCommands(PandemicGame game)
        {
            return game.Board.AdjacentCities[game.CurrentPlayer.Location]
                .Select(city => new DriveFerryCommand(game.CurrentPlayer.Role, city));
        }

        private static IEnumerable<PlayerCommand> CureCommands(PandemicGame game)
        {
            if (!game.CityByName(game.CurrentPlayer.Location).HasResearchStation) yield break;

            foreach (var cureCards in game.CurrentPlayer.Hand
                .CityCards
                .GroupBy(c => c.City.Colour)
                .Where(g => g.Count() >= 5))
            {
                if (!game.CureDiscovered[cureCards.Key])
                    // todo: yield all combinations if > 5 cards
                    yield return new DiscoverCureCommand(cureCards.Take(5).ToArray());
            }
        }

        private static IEnumerable<PlayerCommand> BuildResearchStationCommands(PandemicGame game)
        {
            if (game.ResearchStationPile == 0) yield break;

            if (CurrentPlayerCanBuildResearchStation(game))
                yield return new BuildResearchStationCommand(game.CurrentPlayer.Location);
        }

        private static bool CurrentPlayerCanBuildResearchStation(PandemicGame game)
        {
            if (game.CityByName(game.CurrentPlayer.Location).HasResearchStation)
                return false;

            return game.CurrentPlayer.Hand.CityCards.Any(c => c.City.Name == game.CurrentPlayer.Location);
        }
    }

    public abstract record PlayerCommand
    {
    }

    public record DriveFerryCommand(Role Role, string City) : PlayerCommand;

    public record DiscardPlayerCardCommand(PlayerCard Card) : PlayerCommand;

    public record BuildResearchStationCommand(string City) : PlayerCommand;

    public record DiscoverCureCommand(PlayerCityCard[] Cards) : PlayerCommand;
}
