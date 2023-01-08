﻿using System.Collections.Generic;
using System.Linq;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates.Game;

public partial record PandemicGame
{
    static class PostCommandProcessor
    {
        public static PandemicGame RunGameUntilPlayerCommandIsAvailable(PandemicGame game, List<IEvent> eventList)
        {
            if (game.PhaseOfTurn == TurnPhase.DoActions
                || game.APlayerMustDiscard
                || game.IsOver) return game;

            if (game.PhaseOfTurn == TurnPhase.DrawCards) game = DrawCards(game, eventList);

            if (game.IsOver) return game;

            if (game.APlayerMustDiscard) return game;

            if (game.PhaseOfTurn == TurnPhase.InfectCities) game = InfectCities(game, eventList);

            return game;
        }

        private static PandemicGame InfectCities(PandemicGame game, ICollection<IEvent> events)
        {
            game = InfectCity(game, events);
            if (!game.IsOver) game = InfectCity(game, events);
            if (!game.IsOver)
                game = game.ApplyEvent(new TurnEnded(), events);

            return game;
        }

        private static PandemicGame DrawCards(PandemicGame game, ICollection<IEvent> events)
        {
            if (game.PlayerDrawPile.Count == 0)
                return game.ApplyEvent(new GameLost("No more player cards"), events);

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
}
