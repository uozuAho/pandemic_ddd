using System.Collections.Generic;
using System.Linq;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates.Game;

public partial record PandemicGame
{
    private static PandemicGame DoStuffAfterActions(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        if (game.PlayerDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("No more player cards"), events);

        if (game.PhaseOfTurn == TurnPhase.DrawCards) game = DrawCards(game, events);

        if (game.IsOver) return game;

        if (game.CurrentPlayer.Hand.Count > 7) return game;

        if (game.PhaseOfTurn == TurnPhase.InfectCities)
        {
            game = InfectCity(game, events);
            if (!game.IsOver) game = InfectCity(game, events);
            if (!game.IsOver)
                game = game.ApplyEvent(new TurnEnded(), events);
        }

        return game;
    }

    private static PandemicGame DrawCards(PandemicGame game, ICollection<IEvent> events)
    {
        game = PickUpCard(game, events);

        if (events.Last() is EpidemicTriggered) game = Epidemic(game, events);

        if (game.IsOver) return game;

        if (game.PlayerDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("No more player cards"), events);

        game = PickUpCard(game, events);

        if (events.Last() is EpidemicTriggered) game = Epidemic(game, events);

        return game.ApplyEvent(new TurnPhaseEnded(), events);
    }
}
