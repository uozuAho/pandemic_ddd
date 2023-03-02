using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Commands;
using pandemic.Events;
using pandemic.GameData;
using pandemic.Values;
using utils;

namespace pandemic.Aggregates.Game;

public partial record PandemicGame
{
    public static (PandemicGame, List<IEvent>) CreateNewGame(NewGameOptions options)
    {
        var game = CreateUninitialisedGame(options.Rng);
        var events = new List<IEvent>();

        if (options.Roles.Count is < 2 or > 4)
            throw new GameRuleViolatedException(
                $"number of players must be between 2-4. Was given {options.Roles.Count}");

        game = game
            .SetDifficulty(options.Difficulty, events)
            .SetupInfectionDeck(events)
            .ShufflePlayerDrawPileForDealing(events, options.IncludeSpecialEventCards);

        game = DoInitialInfection(game, events);

        foreach (var role in options.Roles)
        {
            game = game.AddPlayer(role, events);
            game = game.DealPlayerCards(role, InitialPlayerHandSize(options.Roles.Count), events);
        }

        game = game.SetupPlayerDrawPileWithEpidemicCards(events);

        return (game, events);
    }

    public PandemicGame Do(ICommand command, List<IEvent> events)
    {
        var (game, newEvents) = Do(command);

        events.AddRange(newEvents);

        return game;
    }

    public (PandemicGame, IEnumerable<IEvent>) Do(ICommand command)
    {
        if (SelfConsistencyCheckingEnabled) ValidateInternalConsistency();
        PreCommandChecks(command);

        var (game, events) = ExecuteCommand(command);

        var eventList = events.ToList();
        if (CurrentPlayer.ActionsRemaining == 1 && game.CurrentPlayer.ActionsRemaining == 0)
            game = game.ApplyEvent(new TurnPhaseEnded(TurnPhase.DrawCards), eventList);

        while (!game.IsOver && !game.PlayerCommandRequired())
        {
            game = NonPlayerStepper.Step(game, eventList);
        }

        return (game, eventList);
    }

    private void PreCommandChecks(ICommand command)
    {
        ThrowIfGameOver(this);

        var playerWhoMustDiscard = Players.SingleOrDefault(p => p.Hand.Count > 7);
        if (playerWhoMustDiscard != null)
        {
            if (command is not DiscardPlayerCardCommand && command is IPlayerCommand { IsSpecialEvent: false })
                ThrowIfPlayerMustDiscard(playerWhoMustDiscard);
        }

        if (command is IPlayerCommand { ConsumesAction: true } cmd)
        {
            ThrowIfNotRolesTurn(cmd.Role);
            ThrowIfNoActionsRemaining(PlayerByRole(cmd.Role));
        }
    }

    private (PandemicGame, IEnumerable<IEvent>) ExecuteCommand(ICommand command)
    {
        return command switch
        {
            DriveFerryCommand cmd => Do(cmd),
            DiscardPlayerCardCommand cmd => Do(cmd),
            BuildResearchStationCommand cmd => Do(cmd),
            DiscoverCureCommand cmd => Do(cmd),
            DirectFlightCommand cmd => Do(cmd),
            CharterFlightCommand cmd => Do(cmd),
            ShuttleFlightCommand cmd => Do(cmd),
            TreatDiseaseCommand cmd => Do(cmd),
            ShareKnowledgeGiveCommand cmd => Do(cmd),
            ShareKnowledgeTakeCommand cmd => Do(cmd),
            PassCommand cmd => Do(cmd),
            GovernmentGrantCommand cmd => Do(cmd),
            DontUseSpecialEventCommand cmd => Do(cmd),
            EventForecastCommand cmd => Do(cmd),
            AirliftCommand cmd => Do(cmd),
            ResilientPopulationCommand cmd => Do(cmd),
            OneQuietNightCommand cmd => Do(cmd),
            DispatcherMovePawnToOtherPawnCommand cmd => Do(cmd),
            DispatcherDriveFerryPawnCommand cmd => Do(cmd),
            DispatcherDirectFlyPawnCommand cmd => Do(cmd),
            DispatcherCharterFlyPawnCommand cmd => Do(cmd),
            _ => throw new ArgumentOutOfRangeException($"Unsupported action: {command}")
        };
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DispatcherCharterFlyPawnCommand cmd)
    {
        return ApplyEvents(new DispatcherCharterFlewPawn(cmd.PlayerToMove, cmd.Destination));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DispatcherDirectFlyPawnCommand cmd)
    {
        if (cmd.PlayerToMove == Role.Dispatcher)
            throw new GameRuleViolatedException("This command is to move other pawns, not the dispatcher");

        if (!PlayerByRole(Role.Dispatcher).Hand.CityCards.Any(c => c.City.Name == cmd.City))
            throw new GameRuleViolatedException($"Dispatcher doesn't have the {cmd.City} card");

        if (PlayerByRole(cmd.PlayerToMove).Location == cmd.City)
            throw new GameRuleViolatedException($"{cmd.PlayerToMove} is already at {cmd.City}");

        return ApplyEvents(new DispatcherDirectFlewPawn(cmd.PlayerToMove, cmd.City));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DispatcherDriveFerryPawnCommand cmd)
    {
        if (CurrentPlayer.Role != Role.Dispatcher)
            throw new GameRuleViolatedException("It's not the dispatcher's turn");

        if (cmd.PlayerToMove == Role.Dispatcher)
            throw new GameRuleViolatedException("This command is to move other pawns, not the dispatcher");

        var playerToMove = PlayerByRole(cmd.PlayerToMove);

        if (!Board.IsAdjacent(playerToMove.Location, cmd.City))
            throw new GameRuleViolatedException($"{playerToMove.Location} is not next to {cmd.City}");

        return ApplyEvents(new DispatcherDroveFerriedPawn(cmd.PlayerToMove, cmd.City));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DispatcherMovePawnToOtherPawnCommand cmd)
    {
        if (CurrentPlayer.Role != Role.Dispatcher)
            throw new GameRuleViolatedException("It's not the dispatcher's turn");

        if (PlayerByRole(cmd.PlayerToMove).Location == PlayerByRole(cmd.DestinationRole).Location)
            throw new GameRuleViolatedException(
                $"{cmd.PlayerToMove} is already at {PlayerByRole(cmd.DestinationRole).Location}");

        return ApplyEvents(new DispatcherMovedPawnToOther(cmd.PlayerToMove, cmd.DestinationRole));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(OneQuietNightCommand cmd)
    {
        var player = PlayerByRole(cmd.Role);
        if (!player.Hand.Contains(new OneQuietNightCard()))
            throw new GameRuleViolatedException($"{player.Role} doesn't have the one quiet night card");

        return ApplyEvents(new OneQuietNightUsed(cmd.Role));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(ResilientPopulationCommand cmd)
    {
        if (!PlayerByRole(cmd.Role).Hand.Contains(new ResilientPopulationCard()))
            throw new GameRuleViolatedException($"{cmd.Role} doesn't have the resilient population card");

        if (!InfectionDiscardPile.Cards.Contains(cmd.Card))
            throw new GameRuleViolatedException($"{cmd.Card} is not in the discard pile");

        return ApplyEvents(new ResilientPopulationUsed(cmd.Role, cmd.Card));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(AirliftCommand cmd)
    {
        if (!PlayerByRole(cmd.Role).Hand.Contains(new AirliftCard()))
            throw new GameRuleViolatedException($"{cmd.Role} doesn't have the airlift card");

        if (PlayerByRole(cmd.PlayerToMove).Location == cmd.City)
            throw new GameRuleViolatedException($"{cmd.Role} is already at {cmd.City}");

        return ApplyEvents(new AirliftUsed(cmd.Role, cmd.PlayerToMove, cmd.City));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(EventForecastCommand cmd)
    {
        if (!PlayerByRole(cmd.Role).Hand.Contains(new EventForecastCard()))
            throw new GameRuleViolatedException($"{cmd.Role} doesn't have the event forecast card");

        var top6InfectionCards = InfectionDrawPile.Top(6).ToList();
        if (cmd.Cards.Any(c => !top6InfectionCards.Contains(c)))
        {
            throw new GameRuleViolatedException(
                "Not in the top 6 cards of the infection deck: " +
                $"{string.Join(',', top6InfectionCards.Except(cmd.Cards))}");
        }

        return ApplyEvents(new EventForecastUsed(cmd.Role, cmd.Cards));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DontUseSpecialEventCommand cmd)
    {
        if (!Players.Any(p => p.Hand.Any(c => c is ISpecialEventCard)))
            throw new GameRuleViolatedException("No player has a special event card");

        return ApplyEvents(new ChoseNotToUseSpecialEventCard());
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(GovernmentGrantCommand command)
    {
        if (ResearchStationPile == 0) throw new GameRuleViolatedException("No research stations left");

        var player = PlayerByRole(command.Role);
        var card = PlayerByRole(command.Role).Hand.SingleOrDefault(c => c is GovernmentGrantCard);
        if (card is null) throw new GameRuleViolatedException($"{player.Role} doesn't have the government grant card");

        var city = CityByName(command.City);
        if (city.HasResearchStation)
            throw new GameRuleViolatedException("Cannot use government grant on a city with a research station");

        return ApplyEvents(new GovernmentGrantUsed(command.Role, command.City));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(PassCommand command)
    {
        if (CurrentPlayer.Role != command.Role)
            throw new GameRuleViolatedException($"It's not {command.Role}'s turn");

        if (PhaseOfTurn != TurnPhase.DoActions)
            throw new GameRuleViolatedException("You can only pass when you have actions to spend");

        return ApplyEvents(
            new PlayerPassed(command.Role),
            new TurnPhaseEnded(TurnPhase.DrawCards));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DriveFerryCommand command)
    {
        var (role, destination) = command;

        var player = PlayerByRole(role);

        if (!Board.IsCity(destination)) throw new InvalidActionException($"Invalid city '{destination}'");

        if (!Board.IsAdjacent(player.Location, destination))
        {
            throw new GameRuleViolatedException(
                $"Invalid drive/ferry to non-adjacent city: {player.Location} to {destination}");
        }

        return ApplyEvents(new PlayerMoved(role, destination));
    }

    private (PandemicGame game, IEnumerable<IEvent>) Do(CharterFlightCommand cmd)
    {
        var (role, discardCard, destination) = cmd;

        if (!Board.IsCity(destination)) throw new InvalidActionException($"Invalid city '{destination}'");
        if (CurrentPlayer.Role != role) throw new GameRuleViolatedException($"It's not {role}'s turn");
        if (CurrentPlayer.Location == destination)
            throw new GameRuleViolatedException($"You can't charter fly to your current location");

        if (!PlayerByRole(role).Hand.Contains(discardCard))
            throw new GameRuleViolatedException("Current player doesn't have required card");

        if (discardCard.City.Name != PlayerByRole(role).Location)
            throw new GameRuleViolatedException("Discarded card must match current location");

        return ApplyEvents(new PlayerCharterFlewTo(role, destination));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DiscardPlayerCardCommand command)
    {
        var card = command.Card;

        if (!PlayerByRole(command.Role).Hand.Contains(card))
            throw new GameRuleViolatedException("Player doesn't have that card");
        if (PlayerByRole(command.Role).Hand.Count <= 7)
            throw new GameRuleViolatedException("You can't discard if you have less than 8 cards in hand ... I think");

        var (game, events) = ApplyEvents(new PlayerCardDiscarded(command.Role, card));

        return (game, events);
    }

    private (PandemicGame Game, IEnumerable<IEvent> events) Do(BuildResearchStationCommand command)
    {
        var city = command.City;

        if (ResearchStationPile == 0)
            throw new GameRuleViolatedException("No research stations left");
        if (CurrentPlayer.Location != city)
            throw new GameRuleViolatedException($"Player must be in {city} to build research station");
        // ReSharper disable once SimplifyLinqExpressionUseAll nope, this reads better
        if (!CurrentPlayer.Hand.CityCards.Any(c => c.City.Name == city))
            throw new GameRuleViolatedException($"Current player does not have {city} in hand");
        if (CityByName(city).HasResearchStation)
            throw new GameRuleViolatedException($"{city} already has a research station");

        var playerCard = CurrentPlayer.Hand.CityCards.Single(c => c.City.Name == city);

        return ApplyEvents(
            new ResearchStationBuilt(city),
            new PlayerCardDiscarded(command.Role, playerCard)
        );
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DiscoverCureCommand command)
    {
        var cards = command.Cards;

        if (!CityByName(CurrentPlayer.Location).HasResearchStation)
            throw new GameRuleViolatedException("Can only cure at a city with a research station");

        if (cards.Length != 5)
            throw new GameRuleViolatedException("Exactly 5 cards must be used to cure");

        var colour = cards.First().City.Colour;

        if (IsCured(colour))
            throw new GameRuleViolatedException($"{colour} is already cured");

        if (cards.Any(c => c.City.Colour != colour))
            throw new GameRuleViolatedException("Cure: All cards must be the same colour");

        if (cards.Any(c => !CurrentPlayer.Hand.Contains(c)))
            throw new ArgumentException($"given cards contain a card not in player's hand");

        return ApplyEvents(cards
            .Select(c => new PlayerCardDiscarded(command.Role, c))
            .Concat<IEvent>(new[] { new CureDiscovered(colour) }));
    }

    private (PandemicGame, IEnumerable<IEvent>) Do(DirectFlightCommand command)
    {
        var (role, destination) = command;

        if (!CurrentPlayer.Hand.Contains(PlayerCards.CityCard(destination)))
            throw new GameRuleViolatedException("Current player doesn't have required card");

        if (CurrentPlayer.Location == destination)
            throw new GameRuleViolatedException("Cannot direct fly to city you're already in");

        return ApplyEvents(new PlayerDirectFlewTo(role, destination));
    }

    private (PandemicGame game, IEnumerable<IEvent>) Do(ShuttleFlightCommand command)
    {
        var (role, destination) = command;

        if (destination == CurrentPlayer.Location)
            throw new GameRuleViolatedException("Destination can't be current location");

        if (!CityByName(destination).HasResearchStation)
            throw new GameRuleViolatedException($"{destination} doesn't have a research station");

        if (!CityByName(CurrentPlayer.Location).HasResearchStation)
            throw new GameRuleViolatedException($"{destination} doesn't have a research station");

        return ApplyEvents(new PlayerShuttleFlewTo(role, destination));
    }

    private (PandemicGame game, IEnumerable<IEvent>) Do(TreatDiseaseCommand command)
    {
        var (role, city, colour) = command;
        var player = PlayerByRole(role);

        if (player.Location != city)
            throw new GameRuleViolatedException("Can only treat disease in current location");

        if (CityByName(city).Cubes.NumberOf(colour) == 0)
            throw new GameRuleViolatedException("No disease cubes to remove");

        var events = new List<IEvent>();
        var game = ApplyEvent(new TreatedDisease(role, city, colour), events);

        if (game.IsCured(command.Colour) && game.Cities.Sum(c => c.Cubes.NumberOf(command.Colour)) == 0)
            game = game.ApplyEvent(new DiseaseEradicated(command.Colour), events);

        return (game, events);
    }

    private (PandemicGame game, IEnumerable<IEvent>) Do(ShareKnowledgeGiveCommand command)
    {
        var (role, city, receivingRole) = command;
        var giver = PlayerByRole(role);
        var receiver = PlayerByRole(receivingRole);

        if (giver == receiver) throw new GameRuleViolatedException("Cannot share with self!");

        if (!giver.Hand.CityCards.Any(c => c.City.Name == command.City))
            throw new GameRuleViolatedException("Player must have the card to share");

        if (giver.Location != command.City)
            throw new GameRuleViolatedException("Player must be in the city of the given card");

        if (receiver.Location != giver.Location)
            throw new GameRuleViolatedException("Both players must be in the same city");

        return ApplyEvents(new ShareKnowledgeGiven(role, city, receivingRole));
    }

    private (PandemicGame game, IEnumerable<IEvent>) Do(ShareKnowledgeTakeCommand command)
    {
        var (role, city, takeFromRole) = command;
        var taker = PlayerByRole(role);
        var takeFromPlayer = PlayerByRole(takeFromRole);

        if (taker == takeFromPlayer) throw new GameRuleViolatedException("Cannot share with self!");

        if (!takeFromPlayer.Hand.CityCards.Any(c => c.City.Name == command.City))
            throw new GameRuleViolatedException("Player must have the card to share");

        if (taker.Location != command.City)
            throw new GameRuleViolatedException("Player must be in the city of the given card");

        if (takeFromPlayer.Location != taker.Location)
            throw new GameRuleViolatedException("Both players must be in the same city");

        return ApplyEvents(new ShareKnowledgeTaken(role, city, takeFromRole));
    }

    private PandemicGame SetDifficulty(Difficulty difficulty, ICollection<IEvent> events)
    {
        return ApplyEvent(new DifficultySet(difficulty), events);
    }

    private PandemicGame DealPlayerCards(Role role, int numCards, ICollection<IEvent> events)
    {
        var cards = PlayerDrawPile.Top(numCards).ToArray();

        return ApplyEvent(new PlayerCardsDealt(role, cards), events);
    }

    private PandemicGame SetupPlayerDrawPileWithEpidemicCards(ICollection<IEvent> events)
    {
        var drawPile = PlayerDrawPile.Cards
            .OrderBy(_ => Rng.Next())
            .SplitEvenlyInto(NumberOfEpidemicCards(Difficulty))
            .Select(pile => pile
                .Append(new EpidemicCard())
                .OrderBy(_ => Rng.Next()))
            .SelectMany(c => c)
            .ToImmutableList();

        return ApplyEvent(new PlayerDrawPileSetupWithEpidemicCards(drawPile), events);
    }

    private PandemicGame SetupInfectionDeck(ICollection<IEvent> events)
    {
        var unshuffledCities = Board.Cities.Select(InfectionCard.FromCity).OrderBy(_ => Rng.Next());

        return ApplyEvent(new InfectionDeckSetUp(unshuffledCities.ToImmutableList()), events);
    }

    private PandemicGame AddPlayer(Role role, ICollection<IEvent> events)
    {
        return ApplyEvent(new PlayerAdded(role), events);
    }

    private PandemicGame ShufflePlayerDrawPileForDealing(ICollection<IEvent> events, bool includeSpecialEventCards)
    {
        var playerCards = Board.Cities
            .Select(c => new PlayerCityCard(c) as PlayerCard)
            .Concat(includeSpecialEventCards ? SpecialEventCards.All : Enumerable.Empty<PlayerCard>())
            .OrderBy(_ => Rng.Next())
            .ToImmutableList();

        return ApplyEvent(new PlayerDrawPileShuffledForDealing(playerCards), events);
    }

    private static PandemicGame DoInitialInfection(PandemicGame game, ICollection<IEvent> events)
    {
        for (var i = 0; i < 3; i++)
        {
            var infectionCard = game.InfectionDrawPile.TopCard;
            game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);
            for (var j = 0; j < 3; j++)
            {
                game = game.ApplyEvent(new CubeAddedToCity(infectionCard.City, infectionCard.Colour), events);
            }
        }

        for (var i = 0; i < 3; i++)
        {
            var infectionCard = game.InfectionDrawPile.TopCard;
            game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);
            for (var j = 0; j < 2; j++)
            {
                game = game.ApplyEvent(new CubeAddedToCity(infectionCard.City, infectionCard.Colour), events);
            }
        }

        for (var i = 0; i < 3; i++)
        {
            var infectionCard = game.InfectionDrawPile.TopCard;
            game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);
            for (var j = 0; j < 1; j++)
            {
                game = game.ApplyEvent(new CubeAddedToCity(infectionCard.City, infectionCard.Colour), events);
            }
        }

        return game;
    }

    private static PandemicGame PickUpCard(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        if (game.PlayerDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("No more player cards"), events);

        var card = game.PlayerDrawPile.TopCard;

        game = game.ApplyEvent(new PlayerCardPickedUp(card), events);

        if (card is EpidemicCard)
            game = game.ApplyEvent(new EpidemicTriggered(), events);
        else if (game.CardsDrawn == 2)
            game = game.ApplyEvent(new TurnPhaseEnded(TurnPhase.InfectCities), events);

        return game;
    }

    private static PandemicGame InfectCityFromPile(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        if (game.InfectionDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("Ran out of infection cards"), events);

        var infectionCard = game.InfectionDrawPile.TopCard;
        game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);

        if (game.IsEradicated(infectionCard.Colour))
            return game;

        if (game.CityByName(infectionCard.City).Cubes.NumberOf(infectionCard.Colour) == 3)
        {
            return Outbreak(game, infectionCard.City, infectionCard.Colour, events);
        }

        return game.Cubes.NumberOf(infectionCard.Colour) == 0
            ? game.ApplyEvent(new GameLost($"Ran out of {infectionCard.Colour} cubes"), events)
            : game.ApplyEvent(new CubeAddedToCity(infectionCard.City, infectionCard.Colour), events);
    }

    private static PandemicGame Outbreak(PandemicGame game, string city, Colour colour, ICollection<IEvent> events)
    {
        var toOutbreak = new Queue<string>();
        var outBroken = new List<string>();
        toOutbreak.Enqueue(city);

        while (toOutbreak.Count > 0)
        {
            var next = toOutbreak.Dequeue();
            outBroken.Add(next);
            game = game.ApplyEvent(new OutbreakOccurred(next), events);
            if (game.OutbreakCounter == 8)
                return game.ApplyEvent(new GameLost("8 outbreaks"), events);

            var adjacent = game.Board.AdjacentCities[next].Select(game.CityByName).ToList();

            foreach (var adj in adjacent)
            {
                if (adj.Cubes.NumberOf(colour) == 3)
                {
                    if (!outBroken.Contains(adj.Name))
                        toOutbreak.Enqueue(adj.Name);
                }
                else
                {
                    if (game.Cubes.NumberOf(colour) == 0)
                        return game.ApplyEvent(new GameLost($"Ran out of {colour} cubes"), events);
                    game = game.ApplyEvent(new CubeAddedToCity(adj.Name, colour), events);
                }
            }
        }

        return game;
    }

    private static void ThrowIfGameOver(PandemicGame game)
    {
        if (game.IsOver) throw new GameRuleViolatedException("Game is over!");
    }

    private void ThrowIfNotRolesTurn(Role role)
    {
        if (CurrentPlayer.Role != role) throw new GameRuleViolatedException($"It's not {role}'s turn!");
    }

    private static void ThrowIfNoActionsRemaining(Player player)
    {
        if (player.ActionsRemaining == 0)
            throw new GameRuleViolatedException($"Action not allowed: Player {player.Role} has no actions remaining");
    }

    private static void ThrowIfPlayerMustDiscard(Player player)
    {
        if (player.Hand.Count > 7)
            throw new GameRuleViolatedException($"Action not allowed: Player {player.Role} has more than 7 cards in hand");
    }
}
