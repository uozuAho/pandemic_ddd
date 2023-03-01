# Pandemic

The [Pandemic board game](https://en.wikipedia.org/wiki/Pandemic_%28board_game%29),
implemented in C#. Intended for usage by AI agents.

Work in progress! A full game is playable, but many game rules are yet to be
implemented.

# todo
- implement all game rules. left:
  - role special abilities
    - dispatcher: https://boardgames.stackexchange.com/questions/9035/what-are-legal-ways-to-use-the-dispatchers-special-ability-in-pandemic
      - as an action: move any pawn to the location of any other pawn
        - WIP: remove non random options generator
        - add to all commands generator
      - move other players' pawns as if they are your own. including eg charter flight, using own cards
        - drive ferry
        - direct flight
        - charter flight
        - shuttle flight
    - operations expert:
      - build a research station without a city card
      - move from a station to anywhere by discarding a city card
    - medic: remove all cubes of a colour when treating disease, insta-remove cubes when cured
    - researcher: give any city card to a player in the same city
    - quarantine specialist: prevent cube placement and outbreaks in current city and neighbours
    - scientist: cure with 4 cards
    - contingency planner:
      - as an action, take any discarded event card and store it on this card
        - only 1 card can be stored at a time, it's not part of your hand
      - when this event card is played, remove it from the game
- later
  - make pandemic game correct by construction? make all properties get-only
    - hide command and event handlers if not hidden already. pandemic public api should make sense
      in terms of game rules, no internal details
  - dev log: review https://iamwoz.com/blog/20210924_learning_ddd_by_implementing_pandemic
    - any learnings since then?

# Quick start
- install dotnet core (tested with v7)

```sh
# run tests
dotnet test
cd pandemic.console
# Play around with running the game in a console app.
# See Program.cs: it does whatever's not commented out.
dotnet run
```

# What's in this project
- pandemic: core game logic. Immutable & DDD-like.
- pandemic.agents: agents that can play games
- pandemic.console: scratchpad console app
- pandemic.drawing: draw game trees (graphviz dot output)

Check tag `just-before-remove-unused-network-code` for a network game server implementation.

# Notes
- I estimate about 9 x 10^85 possible games of pandemic. This is based on a
  simple calculation of average branching factor and game length. See
  `NumberOfPossibleGamesEstimator`
- mutable2 branch: mutable pandemic aggregate gives 4x speedup in mcts
- it has been shown that determining whether a game is winnable is NP-complete:
  https://www.jstage.jst.go.jp/article/ipsjjip/20/3/20_723/_article. Article
  found via: https://github.com/captn3m0/boardgame-research#pandemic


## Are all games winnable?
This would be good to know. If not all games are winnable, then there's no point
trying to make a bot that can win every game.

Here are some thoughts. Using a simplified game, where there are only city
cards, infection cards, and no special abilities:

An unwinnable case is easy to find if players don't consider each other's hands.
For example, consider a game where the player deck ends up giving all players an
even spread of colours except for one. Each player discards this one colour in
an effort to reach enough cards of another colour to cure it. Quite soon, enough
of the one colour has been discarded that it is impossible to cure that colour.

If each player takes each others' hands into account, and they don't attempt to
collect cards of the same colour, it still seems easy enough to find an
unwinnable configuration of the player deck, however I haven't tried.

Players could also take into account which cards have been discarded, in order
to not discard too many of any one colour. This may increase the odds of
winning, but makes analysing 'winnability' too tedious for my attention span to
handle.

## Alternate implementations
- https://github.com/alexzherdev/pandemic

## Dev log / learnings
- initial start: https://iamwoz.com/blog/20210924_learning_ddd_by_implementing_pandemic
- fuzz testing is great! it has turned up so many bugs that I hadn't covered with simple unit tests
- dunno if 'process managers' are helping. Some commands cause other commands. Eg.
  end of turn can cause a lot of commands and events: epidemics, outbreak chain reactions
  etc. What to do? Just keep this complexity in the command handlers themselves?
  Or, don't call commands from other commands, and instead handle multi-command
  reactions with process managers?
- even though I'm not using event sourcing or even the events emitted by commands,
  they have been very useful to debug complex bugs. Having the entire event history of
  a game makes it easy to see where things have gone wrong. This would be laborious to
  step through in the debugger.
- using the rule to only modify aggregates via events has ensured that all game state
  changes are captured. It's a bit of a pain to add a command + handler + event + handler,
  but I think it's been worth it for the above point alone
- unexplored parts of DDD
  - aggregate design. The entire game state needs to be consistent at all times, so there's
    been no need/opportunity to create smaller aggregates
  - eventual consistency: same reason as above
  - strategic design
- it's frustrating that it still seems to be taking ages to implement a seemingly simple
  game. Maybe it's not simple? What's taking so long? Each special event seems to take about
  1h to code, even now that I've done the first one (which took ... a week!?)

# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
    - alternate: https://docs.google.com/viewer?a=v&pid=sites&srcid=ZGVmYXVsdGRvbWFpbnxzcGlsbGVyZWdsfGd4OjQ2YTMzM2E1NDg4ZGQwNzE
