using System.Collections.Generic;
using System.Linq;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates.Game;

public partial record PandemicGame
{
    private static class PostCommandProcessor
    {
        public static PandemicGame RunGameUntilPlayerCommandIsAvailable(PandemicGame game, List<IEvent> eventList)
        {
            if (game.PhaseOfTurn == TurnPhase.DoActions
                || game.APlayerMustDiscard
                || game.IsOver) return game;

            if (game.Players.Any(p => p.Hand.Any(c => c is ISpecialEventCard)))
            {
                if (game.SkipNextChanceToUseSpecialEvent)
                    game = game with { SkipNextChanceToUseSpecialEvent = false };
                else
                    return game;
            }

            if (game.PhaseOfTurn == TurnPhase.DrawCards) game = DrawCards(game, eventList);

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
            game = PickUpCard(game, events);

            if (game.IsOver) return game;

            if (events.Last() is EpidemicTriggered) game = Epidemic(game, events);

            if (game.IsOver) return game;

            game = PickUpCard(game, events);

            if (game.IsOver) return game;

            if (events.Last() is EpidemicTriggered) game = Epidemic(game, events);

            return game.ApplyEvent(new TurnPhaseEnded(), events);
        }
    }
}
