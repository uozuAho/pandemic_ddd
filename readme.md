# Pandemic

The [Pandemic board game](https://en.wikipedia.org/wiki/Pandemic_%28board_game%29),
implemented in C#. Intended for usage by AI agents.

Work in progress! A full game is playable, but many game rules are yet to be
implemented.

# todo
- charter flight
    - use SetPlayer in other event handlers
- implement all game rules at https://www.ultraboardgames.com/pandemic/game-rules.php
- maybe later
  - need a generic command handler base function? do things required by all commands, eg
    - check if player needs to discard first
    - check if game is over
    - check that player has actions remaining

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
- pandemic.server: host a game of pandemic over a network. Intended for use by
  https://github.com/uozuAho/open_spiel_playground/blob/main/zmq_ttt/client.py

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

# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
