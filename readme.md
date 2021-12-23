# Pandemic

The [Pandemic board game](https://en.wikipedia.org/wiki/Pandemic_%28board_game%29),
implemented in C#. Intended for usage by AI agents.

Work in progress! A full game is playable, but many game rules are yet to be
implemented.

# todo
- implement epidemic
- implement more game rules
- check code todos

# Quick start
- install dotnet core (tested with v6)

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

# notes
- I estimate about 9 x 10^85 possible games of pandemic. This is based on a
  simple calculation of average branching factor and game length. See
  `NumberOfPossibleGamesEstimator`
- mutable2 branch: mutable pandemic aggregate gives 4x speedup in mcts
- it has been shown that determining whether a game is winnable is NP-complete:
  https://www.jstage.jst.go.jp/article/ipsjjip/20/3/20_723/_article. Article
  found via: https://github.com/captn3m0/boardgame-research#pandemic

# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
