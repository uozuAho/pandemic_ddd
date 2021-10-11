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
        public IEnumerable<PlayerCommand> LegalMoves(PandemicGame game)
        {
            if (game.IsOver) yield break;

            if (game.CurrentPlayer.Hand.Count > 7)
            {
                foreach (var card in game.CurrentPlayer.Hand)
                {
                    yield return new DiscardPlayerCardCommand(card);
                }
                yield break;
            }

            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                foreach (var city in game.Board.AdjacentCities[game.CurrentPlayer.Location])
                {
                    yield return new DriveFerryCommand(game.CurrentPlayer.Role, city);
                }
                // todo: yield build research station
            }
        }
    }

    public abstract record PlayerCommand
    {
    }

    public record DriveFerryCommand(Role Role, string City) : PlayerCommand;

    public record DiscardPlayerCardCommand(PlayerCard Card) : PlayerCommand;
}
