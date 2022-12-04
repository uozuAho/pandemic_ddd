using System.Linq;
using System.Text;
using pandemic.Aggregates;
using pandemic.Aggregates.Game;

namespace pandemic
{
    public class PandemicGameStringRenderer
    {
        public static string FullState(PandemicGame game)
        {
            var sb = new StringBuilder();
            if (game.IsWon) sb.AppendLine("Game won!");
            else sb.AppendLine("Game lost. " + game.LossReason);
            sb.AppendLine($"infection rate: {game.InfectionRate}");
            sb.AppendLine($"outbreak counter: {game.OutbreakCounter}");
            sb.AppendLine($"cube piles: {string.Join(' ', game.Cubes)}");
            sb.AppendLine($"cards on player draw pile: {game.PlayerDrawPile.Count}");
            sb.AppendLine($"cards on infection deck: {game.InfectionDrawPile.Count}");
            sb.AppendLine($"cured: {string.Join(' ', game.CureDiscovered)}");
            sb.AppendLine($"current player: {game.CurrentPlayer.Role}");
            sb.AppendLine($"  location: {game.CurrentPlayer.Location}");
            sb.AppendLine($"  remaining actions: {game.CurrentPlayer.ActionsRemaining}");
            sb.AppendLine($"  hand: {string.Join("\n    ", game.CurrentPlayer.Hand)}");
            sb.AppendLine("cities with cubes:");
            foreach (var city in game.Cities)
            {
                if (city.Cubes.Any(c => c.Value > 0))
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
            sb.AppendLine($"cured: {string.Join(' ', game.CureDiscovered)}");
            foreach (var player in game.Players)
            {
                var colourCounts = string.Join(",", player.Hand.CityCards
                    .GroupBy(c => c.City.Colour)
                    .Select(g => $"{g.Key}:{g.Count()}"));

                sb.AppendLine($"{player.Role}:{player.Location}:{colourCounts}");
            }

            return sb.ToString();
        }
    }
}
