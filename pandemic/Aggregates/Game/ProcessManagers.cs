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

        if (game.PhaseOfTurn == TurnPhase.DrawCards)
        {
            game = PickUpCard(game, events);

            if (((PlayerCardPickedUp)events.Last()).Card is EpidemicCard epidemicCard)
                game = Epidemic(game, epidemicCard, events);

            if (game.IsOver) return game;

            if (game.PlayerDrawPile.Count == 0)
                return game.ApplyEvent(new GameLost("No more player cards"), events);

            game = PickUpCard(game, events);

            if (((PlayerCardPickedUp)events.Last()).Card is EpidemicCard epidemicCard2)
                game = Epidemic(game, epidemicCard2, events);

            game = game.ApplyEvent(new TurnPhaseEnded(), events);
        }

        if (game.IsOver) return game;

        if (game.CurrentPlayer.Hand.Count > 7)
            return game;

        if (game.PhaseOfTurn == TurnPhase.InfectCities)
        {
            game = InfectCities(game, events);
            if (!game.IsOver)
                game = game.ApplyEvent(new TurnEnded(), events);
        }

        return game;
    }
}
