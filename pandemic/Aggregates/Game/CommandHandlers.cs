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
        var game = CreateUninitialisedGame();
        var events = new List<IEvent>();

        if (options.Roles.Count < 2 || options.Roles.Count > 4)
            throw new GameRuleViolatedException(
                $"number of players must be between 2-4. Was given {options.Roles.Count}");

        game = game
            .SetDifficulty(options.Difficulty, events)
            .SetInfectionRate(2, events)
            .SetOutbreakCounter(0, events)
            .SetupInfectionDeck(events)
            .ShufflePlayerDrawPileForDealing(events);

        game = DoInitialInfection(game, events);

        foreach (var role in options.Roles)
        {
            game = game.AddPlayer(role, events);
            game = game.DealPlayerCards(role, InitialPlayerHandSize(options.Roles.Count), events);
        }

        game = game.SetupPlayerDrawPileWithEpidemicCards(events);

        return (game, events);
    }

    public (PandemicGame, IEnumerable<IEvent>) Do(PlayerCommand action)
    {
        return action switch
        {
            DriveFerryCommand command => DriveOrFerryPlayer(command.Role, command.City),
            DiscardPlayerCardCommand command => DiscardPlayerCard(command.Card),
            BuildResearchStationCommand command => BuildResearchStation(command.City),
            DiscoverCureCommand command => DiscoverCure(command.Cards),
            DirectFlightCommand command => DirectFlight(command.Role, command.City),
            CharterFlightCommand command => CharterFlight(command.Role, command.City),
            _ => throw new ArgumentOutOfRangeException($"Unsupported action: {action}")
        };
    }

    public (PandemicGame, IEnumerable<IEvent>) DriveOrFerryPlayer(Role role, string city)
    {
        ThrowIfGameOver(this);
        ThrowIfNotRolesTurn(role);
        ThrowIfNoActionsRemaining(CurrentPlayer);
        ThrowIfPlayerMustDiscard(CurrentPlayer);

        var player = PlayerByRole(role);

        if (!Board.IsCity(city)) throw new InvalidActionException($"Invalid city '{city}'");

        if (!Board.IsAdjacent(player.Location, city))
        {
            throw new InvalidActionException(
                $"Invalid drive/ferry to non-adjacent city: {player.Location} to {city}");
        }

        return ApplyAndEndTurnIfNeeded(new[] {new PlayerMoved(role, city)});
    }

    public (PandemicGame game, IEnumerable<IEvent>) CharterFlight(Role role, string city)
    {
        if (!Board.IsCity(city)) throw new InvalidActionException($"Invalid city '{city}'");
        if (CurrentPlayer.Role != role) throw new GameRuleViolatedException($"It's not {role}'s turn");

        var player = PlayerByRole(role);

        if (!PlayerByRole(role).Hand.Contains(PlayerCards.CityCard(player.Location)))
            throw new GameRuleViolatedException("Current player doesn't have required card");

        return ApplyAndEndTurnIfNeeded(new [] {new PlayerCharterFlewTo(role, city)});
    }

    public (PandemicGame, IEnumerable<IEvent>) DiscardPlayerCard(PlayerCard card)
    {
        ThrowIfGameOver(this);

        var (game, events) = ApplyEvents(new PlayerCardDiscarded(card));

        if (game.CurrentPlayer.ActionsRemaining == 0 && game.CurrentPlayer.Hand.Count <= 7)
            game = InfectCities(game, events);

        return (game, events);
    }

    public (PandemicGame Game, IEnumerable<IEvent> events) BuildResearchStation(string city)
    {
        ThrowIfGameOver(this);
        ThrowIfNoActionsRemaining(CurrentPlayer);
        ThrowIfPlayerMustDiscard(CurrentPlayer);

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

        return ApplyAndEndTurnIfNeeded(new List<IEvent>
        {
            new ResearchStationBuilt(city),
            new PlayerCardDiscarded(playerCard)
        });
    }

    public (PandemicGame, IEnumerable<IEvent>) DiscoverCure(PlayerCityCard[] cards)
    {
        ThrowIfGameOver(this);
        ThrowIfNoActionsRemaining(CurrentPlayer);
        ThrowIfPlayerMustDiscard(CurrentPlayer);

        if (!CityByName(CurrentPlayer.Location).HasResearchStation)
            throw new GameRuleViolatedException("Can only cure at a city with a research station");

        if (cards.Length != 5)
            throw new GameRuleViolatedException("Exactly 5 cards must be used to cure");

        var colour = cards.First().City.Colour;

        if (CureDiscovered[colour])
            throw new GameRuleViolatedException($"{colour} is already cured");

        if (cards.Any(c => c.City.Colour != colour))
            throw new GameRuleViolatedException("Cure: All cards must be the same colour");

        if (cards.Any(c => !CurrentPlayer.Hand.Contains(c)))
            throw new ArgumentException($"given cards contain a card not in player's hand");

        return ApplyAndEndTurnIfNeeded(cards
            .Select(c => new PlayerCardDiscarded(c))
            .Concat<IEvent>(new[] { new CureDiscovered(colour) }));
    }

    public (PandemicGame, IEnumerable<IEvent>) DirectFlight(Role currentPlayerRole, string city)
    {
        if (!CurrentPlayer.Hand.Contains(PlayerCards.CityCard(city)))
            throw new GameRuleViolatedException("Current player doesn't have required card");

        if (CurrentPlayer.Location == city)
            throw new GameRuleViolatedException("Cannot direct fly to city you're already in");

        return ApplyAndEndTurnIfNeeded(new [] {new PlayerDirectFlewTo(currentPlayerRole, city)});
    }

    public (PandemicGame game, IEnumerable<IEvent>) ShuttleFlight(Role role, string city)
    {
        ThrowIfGameOver(this);
        ThrowIfNotRolesTurn(role);
        ThrowIfNoActionsRemaining(CurrentPlayer);
        ThrowIfPlayerMustDiscard(CurrentPlayer);

        return ApplyAndEndTurnIfNeeded(new[] { new PlayerShuttleFlewTo(role, city) });
    }

    private (PandemicGame, IEnumerable<IEvent>) ApplyAndEndTurnIfNeeded(IEnumerable<IEvent> events)
    {
        var (game, eventList) = ApplyEvents(events);

        if (game.CurrentPlayer.ActionsRemaining == 0 && !game.IsOver)
            game = DoStuffAfterActions(game, eventList);

        return (game, eventList);
    }

    private static PandemicGame InfectCities(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        game = InfectCity(game, events);
        if (!game.IsOver) game = InfectCity(game, events);
        if (!game.IsOver) game = game.ApplyEvent(new TurnEnded(), events);
        return game;
    }

    private PandemicGame SetDifficulty(Difficulty difficulty, ICollection<IEvent> events)
    {
        return ApplyEvent(new DifficultySet(difficulty), events);
    }

    private PandemicGame SetInfectionRate(int rate, ICollection<IEvent> events)
    {
        return ApplyEvent(new InfectionRateSet(rate), events);
    }

    private PandemicGame SetOutbreakCounter(int value, ICollection<IEvent> events)
    {
        return ApplyEvent(new OutbreakCounterSet(value), events);
    }

    private PandemicGame DealPlayerCards(Role role, int numCards, ICollection<IEvent> events)
    {
        var cards = PlayerDrawPile.Top(numCards).ToArray();

        return ApplyEvent(new PlayerCardsDealt(role, cards), events);
    }

    private PandemicGame SetupPlayerDrawPileWithEpidemicCards(ICollection<IEvent> events)
    {
        var rng = new Random();

        var drawPile = PlayerDrawPile.Cards
            .OrderBy(_ => rng.Next())
            .SplitEvenlyInto(NumberOfEpidemicCards(Difficulty))
            .Select(pile => pile
                .Append(new EpidemicCard())
                .OrderBy(_ => rng.Next()))
            .SelectMany(c => c)
            .ToImmutableList();

        return ApplyEvent(new PlayerDrawPileSetupWithEpidemicCards(drawPile), events);
    }

    private PandemicGame SetupInfectionDeck(ICollection<IEvent> events)
    {
        var rng = new Random();
        var unshuffledCities = Board.Cities.Select(c => new InfectionCard(c)).OrderBy(_ => rng.Next());

        return ApplyEvent(new InfectionDeckSetUp(unshuffledCities.ToImmutableList()), events);
    }

    private PandemicGame AddPlayer(Role role, ICollection<IEvent> events)
    {
        return ApplyEvent(new PlayerAdded(role), events);
    }

    private PandemicGame ShufflePlayerDrawPileForDealing(ICollection<IEvent> events)
    {
        var rng = new Random();

        var playerCards = Board.Cities
            .Select(c => new PlayerCityCard(c) as PlayerCard)
            .OrderBy(_ => rng.Next())
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
                game = game.ApplyEvent(new CubeAddedToCity(infectionCard.City), events);
            }
        }

        for (var i = 0; i < 3; i++)
        {
            var infectionCard = game.InfectionDrawPile.TopCard;
            game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);
            for (var j = 0; j < 2; j++)
            {
                game = game.ApplyEvent(new CubeAddedToCity(infectionCard.City), events);
            }
        }

        for (var i = 0; i < 3; i++)
        {
            var infectionCard = game.InfectionDrawPile.TopCard;
            game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);
            for (var j = 0; j < 1; j++)
            {
                game = game.ApplyEvent(new CubeAddedToCity(infectionCard.City), events);
            }
        }

        return game;
    }

    private static PandemicGame DoStuffAfterActions(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        if (game.PlayerDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("No more player cards"), events);

        game = PickUpCard(game, events);

        if (game.PlayerDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("No more player cards"), events);

        game = PickUpCard(game, events);

        if (game.CurrentPlayer.Hand.Count > 7)
            return game;

        game = InfectCities(game, events);

        return game;
    }

    private static PandemicGame PickUpCard(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        var card = game.PlayerDrawPile.TopCard;

        game = game.ApplyEvent(new PlayerCardPickedUp(card), events);

        if (card is EpidemicCard epidemicCard)
            game = Epidemic(game, epidemicCard, events);

        return game;
    }

    private static PandemicGame Epidemic(PandemicGame game, EpidemicCard card, ICollection<IEvent> events)
    {
        return game.ApplyEvent(new EpidemicCardDiscarded(game.CurrentPlayer, card), events);

        // todo: game rules: handle epidemic
    }

    private static PandemicGame InfectCity(PandemicGame game, ICollection<IEvent> events)
    {
        ThrowIfGameOver(game);

        if (game.InfectionDrawPile.Count == 0)
            return game.ApplyEvent(new GameLost("Ran out of infection cards"), events);

        var infectionCard = game.InfectionDrawPile.TopCard;
        game = game.ApplyEvent(new InfectionCardDrawn(infectionCard), events);

        return game.Cubes[infectionCard.City.Colour] == 0
            ? game.ApplyEvent(new GameLost($"Ran out of {infectionCard.City.Colour} cubes"), events)
            : game.ApplyEvent(new CubeAddedToCity(infectionCard.City), events);
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
