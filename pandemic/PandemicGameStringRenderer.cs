using System.Collections.Generic;
using System.Linq;
using System.Text;
using pandemic.Aggregates.Game;
using pandemic.Values;

namespace pandemic
{
    public class PandemicGameStringRenderer
    {
        public static string FullState(PandemicGame game)
        {
            var sb = new StringBuilder();
            if (game.IsOver)
            {
                if (game.IsWon) sb.AppendLine("Game won!");
                else sb.AppendLine("Game lost. " + game.LossReason);
            }
            else
            {
                sb.AppendLine("Game not over");
            }
            sb.AppendLine($"current player: {game.CurrentPlayer.Role}");
            if (game.PhaseOfTurn == TurnPhase.DrawCards)
                sb.AppendLine($"turn phase: {game.PhaseOfTurn}, {game.CardsDrawn} cards drawn");
            else
                sb.AppendLine($"turn phase: {game.PhaseOfTurn}");
            sb.AppendLine($"infection rate: {game.InfectionRate}");
            sb.AppendLine($"outbreak counter: {game.OutbreakCounter}");
            sb.AppendLine($"cube piles: {string.Join(' ', game.Cubes)}");
            sb.AppendLine($"cards on player draw pile: {game.PlayerDrawPile.Count}");
            sb.AppendLine($"cards on infection deck: {game.InfectionDrawPile.Count}");
            sb.AppendLine($"diseases cured: {string.Join(' ', game.CuresDiscovered.Select(c => c.Colour))}");
            foreach (var player in game.Players)
            {
                sb.AppendLine($"player: {player.Role}");
                sb.AppendLine($"  location: {player.Location}");
                sb.AppendLine($"  remaining actions: {player.ActionsRemaining}");
                sb.AppendLine($"  hand: {string.Join("\n    ", player.Hand)}");
            }
            sb.AppendLine("cities with cubes:");
            foreach (var city in game.Cities.OrderByDescending(c => c.MaxNumCubes()))
            {
                if (city.Cubes.Any())
                {
                    sb.AppendLine($"  {city.Name.PadRight(12)}: {string.Join(' ', city.Cubes)}");
                }
            }

            return sb.ToString();
        }

        public static string ShortState(PandemicGame game)
        {
            var sb = new StringBuilder();
            if (game.IsWon) sb.AppendLine("Game won!");
            else if (game.IsLost) sb.AppendLine("Game lost. " + game.LossReason);
            sb.AppendLine($"outbreak counter: {game.OutbreakCounter}");
            sb.AppendLine($"cards on player draw pile: {game.PlayerDrawPile.Count}");
            sb.AppendLine($"cards on infection deck: {game.InfectionDrawPile.Count}");
            sb.AppendLine($"cured: {string.Join(' ', game.CuresDiscovered.Select(c => c.Colour))}");
            foreach (var player in game.Players)
            {
                var colourCounts = string.Join(",", player.Hand.CityCards()
                    .GroupBy(c => c.City.Colour)
                    .Select(g => $"{g.Key}:{g.Count()}"));

                sb.AppendLine($"{player.Role}:{player.Location}:{colourCounts}");
            }

            return sb.ToString();
        }
    }
}
