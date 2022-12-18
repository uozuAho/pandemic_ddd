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
}
