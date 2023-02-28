using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Events;
using pandemic.Values;
using utils;

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
                TurnPhase.EpidemicIntensify => EpidemicIntensify(game, eventList),
                TurnPhase.InfectCities => InfectCities(game, eventList),
                TurnPhase.DoActions => throw new InvalidOperationException("Player command?"),
                _ => throw new InvalidOperationException("Shouldn't get here")
            };
        }

        private static PandemicGame DrawCards(PandemicGame game, ICollection<IEvent> events)
        {
            return game.CardsDrawn == 2
                ? game.ApplyEvent(new TurnPhaseEnded(TurnPhase.InfectCities), events)
                : PickUpCard(game, events);
        }

        private static PandemicGame Epidemic(PandemicGame game, ICollection<IEvent> events)
        {
            var epidemicInfectionCard = game.InfectionDrawPile.BottomCard;
            var epidemicCard = (EpidemicCard)game.CurrentPlayer.Hand.Single(c => c is EpidemicCard);

            game = game.ApplyEvent(new EpidemicPlayerCardDiscarded(game.CurrentPlayer, epidemicCard), events);

            // increase the infection rate
            game = game.ApplyEvent(new InfectionRateIncreased(), events);

            // infect: add 3 cubes to epidemic city
            if (game.Cubes.NumberOf(epidemicInfectionCard.Colour) < 3)
                return game.ApplyEvent(new GameLost($"Ran out of {epidemicInfectionCard.Colour} cubes"), events);

            for (var i = 0; i < 3; i++)
            {
                if (game.CityByName(epidemicInfectionCard.City).Cubes.NumberOf(epidemicInfectionCard.Colour) < 3)
                    game = game.ApplyEvent(
                        new CubeAddedToCity(epidemicInfectionCard.City, epidemicInfectionCard.Colour), events);
                else
                {
                    game = Outbreak(game, epidemicInfectionCard.City, epidemicInfectionCard.Colour, events);
                    break;
                }
            }

            game = game.ApplyEvent(new EpidemicInfectionCardDiscarded(epidemicInfectionCard), events);
            game = game.ApplyEvent(new EpidemicCityInfected(), events);
            game = game.ApplyEvent(new TurnPhaseEnded(TurnPhase.EpidemicIntensify), events);

            return game;
        }

        private static PandemicGame EpidemicIntensify(PandemicGame game, ICollection<IEvent> events)
        {
            var shuffledDiscardPile = game.InfectionDiscardPile.Cards.Shuffle(game.Rng).ToList();
            game = game.ApplyEvent(new EpidemicIntensified(shuffledDiscardPile), events);
            return game.ApplyEvent(new TurnPhaseEnded(TurnPhase.DrawCards), events);
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
    }
}
