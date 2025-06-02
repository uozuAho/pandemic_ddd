namespace pandemic;

using System.Linq;
using System.Text;
using Aggregates.Game;
using Values;

public static class PandemicGameStringRenderer
{
    public static string FullState(PandemicGame game)
    {
        var sb = new StringBuilder();
        _ = game.IsOver
            ? game.IsWon
                ? sb.AppendLine("Game won!")
                : sb.AppendLine("Game lost. " + game.LossReason)
            : sb.AppendLine("Game not over");

        _ = sb.AppendLine($"current player: {game.CurrentPlayer.Role}");

        _ =
            game.PhaseOfTurn == TurnPhase.DrawCards
                ? sb.AppendLine($"turn phase: {game.PhaseOfTurn}, {game.CardsDrawn} cards drawn")
                : sb.AppendLine($"turn phase: {game.PhaseOfTurn}");

        _ = sb.AppendLine($"infection rate: {game.InfectionRate}");
        _ = sb.AppendLine($"outbreak counter: {game.OutbreakCounter}");
        _ = sb.AppendLine($"cube piles: {string.Join(' ', game.Cubes)}");
        _ = sb.AppendLine($"cards on player draw pile: {game.PlayerDrawPile.Count}");
        _ = sb.AppendLine($"cards on infection deck: {game.InfectionDrawPile.Count}");
        _ = sb.AppendLine(
            $"diseases cured: {string.Join(' ', game.CuresDiscovered.Select(c => c.Colour))}"
        );
        foreach (var player in game.Players)
        {
            _ = sb.AppendLine($"player: {player.Role}");
            _ = sb.AppendLine($"  location: {player.Location}");
            _ = sb.AppendLine($"  remaining actions: {player.ActionsRemaining}");
            _ = sb.AppendLine($"  hand: {string.Join("\n    ", player.Hand)}");
        }
        _ = sb.AppendLine("cities with cubes:");
        foreach (var city in game.Cities.OrderByDescending(c => c.MaxNumCubes()))
        {
            if (city.Cubes.Any())
            {
                _ = sb.AppendLine($"  {city.Name.PadRight(12)}: {string.Join(' ', city.Cubes)}");
            }
        }

        return sb.ToString();
    }

    public static string ShortState(PandemicGame game)
    {
        var sb = new StringBuilder();
        if (game.IsWon)
        {
            _ = sb.AppendLine("Game won!");
        }
        else if (game.IsLost)
        {
            _ = sb.AppendLine("Game lost. " + game.LossReason);
        }

        _ = sb.AppendLine($"outbreak counter: {game.OutbreakCounter}");
        _ = sb.AppendLine($"cards on player draw pile: {game.PlayerDrawPile.Count}");
        _ = sb.AppendLine($"cards on infection deck: {game.InfectionDrawPile.Count}");
        _ = sb.AppendLine($"cured: {string.Join(' ', game.CuresDiscovered.Select(c => c.Colour))}");
        foreach (var player in game.Players)
        {
            var colourCounts = string.Join(
                ",",
                player
                    .Hand.CityCards()
                    .GroupBy(c => c.City.Colour)
                    .Select(g => $"{g.Key}:{g.Count()}")
            );

            _ = sb.AppendLine($"{player.Role}:{player.Location}:{colourCounts}");
        }

        return sb.ToString();
    }
}
