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
            if (game.PlayerCommandRequired()) return game;

            if (game.PhaseOfTurn == TurnPhase.DrawCards) game = DrawCards(game, eventList);

            if (game.PhaseOfTurn == TurnPhase.Epidemic) game = Epidemic(game, eventList);

            if (game.IsOver) return game;

            if (game.APlayerMustDiscard) return game;

            if (game.PhaseOfTurn == TurnPhase.InfectCities)
            {
                game = InfectCities(game, eventList);
                if (!game.IsOver)
                    game = game.ApplyEvent(new TurnEnded(), eventList);
            }

            return game;
        }

        private static PandemicGame InfectCities(PandemicGame game, ICollection<IEvent> events)
        {
            for (var i = 0; i < game.InfectionRate; i++)
            {
                if (!game.IsOver) game = InfectCityFromPile(game, events);
            }
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
