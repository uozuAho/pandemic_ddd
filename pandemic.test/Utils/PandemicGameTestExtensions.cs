using System.Collections.Immutable;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.Values;

namespace pandemic.test.Utils;

public static class PandemicGameTestExtensions
{
    public static PandemicGame SetCurrentPlayerAs(this PandemicGame game, Player player)
    {
        return game with
        {
            Players = game.Players.Replace(game.CurrentPlayer, player)
        };
    }

    public static PandemicGame SetPlayer(this PandemicGame game, Role role, Player player)
    {
        return game with { Players = game.Players.Replace(game.PlayerByRole(role), player) };
    }

    public static PandemicGame WithNoEpidemics(this PandemicGame game)
    {
        return game with
        {
            PlayerDrawPile = new Deck<PlayerCard>(game.PlayerDrawPile.Cards.Where(c => c is not EpidemicCard))
        };
    }

    public static PandemicGame RemoveAllCubesFromCities(this PandemicGame game)
    {
        return game with
        {
            Cities = game.Cities.Select(c => c with { Cubes = CubePile.Empty }).ToImmutableList()
        };
    }
}
