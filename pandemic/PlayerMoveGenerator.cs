using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic
{
    public class PlayerMoveGenerator
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

            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                foreach (var city in game.Board.AdjacentCities[game.CurrentPlayer.Location])
                {
                    yield return new DriveFerryPlayerCommand(game.CurrentPlayer.Role, city);
                }
            }

            if (game.CurrentPlayer.Hand.Count > 7)
            {
                foreach (var card in game.CurrentPlayer.Hand)
                {
                    yield return new DiscardPlayerCardCommand(card);
                }
            }
        }
    }

    public abstract record PlayerCommand
    {
    }

    public record DriveFerryPlayerCommand(Role Role, string City) : PlayerCommand;

    public record DiscardPlayerCardCommand(PlayerCard Card) : PlayerCommand;
}
