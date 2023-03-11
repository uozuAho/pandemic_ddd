# Pandemic

The [Pandemic board game](https://en.wikipedia.org/wiki/Pandemic_%28board_game%29),
implemented in C#. Intended for usage by AI agents.

# todo
- fuzz fail: disease not eradicated. steps:
  - medic cures blue (at atlanta?)
  - drive to chicago, autoremove
  - drive to montreal, autoremove, eradicated
  - drive to new york, autoremove, eradicated (UNEXPECTED)
- win one game at any difficulty, any strategy
  - strategy
    - inline todos
    - look at what agent is doing
      - not much treating disease, lots of moving back/forth between same cities
    - play with game evaluator. Try to lose in some other way to outbreaks/cubes
    - check BGA replays: any clear strategy?
    - make game easier: eg. no epidemics, no outbreaks
  - make faster: play more games, search more game states
    - goal: single threaded: 100 games/sec (non-search), 5000 states/sec (search)
    - ideas
      - mutable?
- can a heroic game be won?
  - pandemic strategies
    - easily win on 6, no strat mentioned: https://www.reddit.com/r/boardgames/comments/7zk0dr/how_difficult_is_it_to_win_pandemic_with_6/
    - indicates 6 is possible, some strats: https://boardgamegeek.com/thread/2356305/questions-pandemic-base-game-heroic-mode
        - clear cubes early game, while building hands
        - more players: easier to clear cubes, harder to cure. Inverse for fewer players
    - read more:
        - BGA has replays! stats here: https://forum.boardgamearena.com/viewtopic.php?t=25373
        - https://diceboardcards.wordpress.com/2013/08/16/how-to-win-pandemic-on-hard-mode-heroic-a-review/
        - https://boardgames.stackexchange.com/questions/2372/what-are-good-general-strategies-for-pandemic
- later
  - make pandemic game correct by construction? make all properties get-only
    - hide command and event handlers if not hidden already. pandemic public api should make sense
      in terms of game rules, no internal details

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


# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
    - alternate: https://docs.google.com/viewer?a=v&pid=sites&srcid=ZGVmYXVsdGRvbWFpbnxzcGlsbGVyZWdsfGd4OjQ2YTMzM2E1NDg4ZGQwNzE
