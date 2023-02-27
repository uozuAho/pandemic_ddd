using System;
using System.Collections.Generic;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates.Game;

public partial record PandemicGame
{
    private static class NonPlayerStepper
    {
        public static PandemicGame Step(PandemicGame game, List<IEvent> eventList)
        {
            if (game.PlayerCommandRequired() || game.IsOver) return game;

            return game.PhaseOfTurn switch
            {
                TurnPhase.DrawCards => DrawCards(game, eventList),
                TurnPhase.Epidemic => Epidemic(game, eventList),
                TurnPhase.InfectCities => InfectCities(game, eventList),
                TurnPhase.DoActions => throw new InvalidOperationException("Player command?"),
                _ => throw new InvalidOperationException("Shouldn't get here")
            };
        }

        private static PandemicGame InfectCities(PandemicGame game, ICollection<IEvent> events)
        {
            for (var i = 0; i < game.InfectionRate; i++)
            {
                if (!game.IsOver) game = InfectCityFromPile(game, events);
            }

            if (!game.IsOver)
                game = game.ApplyEvent(new TurnEnded(), events);

            return game;
        }

        private static PandemicGame DrawCards(PandemicGame game, ICollection<IEvent> events)
        {
            if (game.CardsDrawn == 2) return game.ApplyEvent(new TurnPhaseEnded(), events);

            game = PickUpCard(game, events);

            if (game.IsOver) return game;

            if (game.PhaseOfTurn == TurnPhase.Epidemic) return game;

            if (game.IsOver) return game;

            if (game.PlayerCommandRequired()) return game;

            if (game.CardsDrawn == 2) return game.ApplyEvent(new TurnPhaseEnded(), events);

            game = PickUpCard(game, events);

            if (game.IsOver) return game;

            if (game.PhaseOfTurn == TurnPhase.Epidemic) return game;

            return game.ApplyEvent(new TurnPhaseEnded(), events);
        }
    }
}
