using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic
{
    class PlayerMoveGenerator
    {
        /// <summary>
        /// Determines available 'moves' from the given game state. 'Moves' are any
        /// game action that requires a player to do something, eg:
        /// - performing an action by their role
        /// - playing an event card
        /// - drawing player cards
        /// </summary>
        public IEnumerable<PlayerMove> LegalMoves(PandemicGame game)
        {
            if (game.IsOver) yield break;

            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                foreach (var city in game.Board.AdjacentCities[game.CurrentPlayer.Location])
                {
                    yield return new PlayerMove(game.CurrentPlayer.Role, MoveType.DriveOrFerry, city);
                }
            }
        }
    }

    public enum MoveType
    {
        DriveOrFerry
    }

    public record PlayerMove(Role Role, MoveType MoveType, string City);
}
