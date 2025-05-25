# Pandemic

The [Pandemic board game](https://en.wikipedia.org/wiki/Pandemic_%28board_game%29),
implemented in C#. Intended for usage by AI agents.

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
- pandemic: core game logic. Immutable & DDD-inspired
- pandemic.agents: agents that can play games
- pandemic.console: scratchpad console app. Uncomment stuff in Program.cs to run it.
- pandemic.perftest: repeatable test runs for profiling and benchmarking
- pandemic.drawing: draw game trees (graphviz dot output)
- utils: C# language/library utils

Check tag `just-before-remove-unused-network-code` for a network game server implementation.

# Notes
## Are all games winnable?
This would be good to know. If not all games are winnable, then there's no point
trying to make a bot that can win every game.

It has been shown that determining whether a game is winnable is NP-complete:
https://www.jstage.jst.go.jp/article/ipsjjip/20/3/20_723/_article. Article
found via: https://github.com/captn3m0/boardgame-research#pandemic

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

## Agent performance (ie. ability to win)
Uncomment `WinLossStats.PlayGamesAndPrintWinLossStats` to get these stats. So far,
the best performance I've got is from playing many games with `GreedyAgent`s. This
agent picks the 'best' move based on a `GameEvaluator`, which is just a scorer I
wrote to indicate how 'good' a game state is.

Other agents like greedy best-first, DFS run forever without finding a win. I think
the app needs some performance improvements to make search-based agents more viable.
Also, I think there is quite a bit of luck involved in winning a game, so searching
is futile for unwinnable games.

`PlayGamesAndPrintWinLossStats` prints out statistics similar to https://forum.boardgamearena.com/viewtopic.php?t=25373

Greedy agent performance for 2 player games:

Role stats:
Medic: 29 wins, 233 losses (11.1%)
OperationsExpert: 25 wins, 228 losses (9.9%)
Researcher: 24 wins, 235 losses (9.3%)
QuarantineSpecialist: 21 wins, 237 losses (8.1%)
Scientist: 20 wins, 257 losses (7.2%)
ContingencyPlanner: 12 wins, 271 losses (4.2%)
Dispatcher: 11 wins, 259 losses (4.1%)

Team stats:
M, R      : 11 wins, 33 losses (25.0%)
M, O      : 8 wins, 30 losses (21.1%)
Q, R      : 6 wins, 32 losses (15.8%)
O, S      : 6 wins, 41 losses (12.8%)
O, Q      : 5 wins, 41 losses (10.9%)
C, S      : 4 wins, 42 losses (8.7%)
D, M      : 4 wins, 45 losses (8.2%)
R, S      : 4 wins, 46 losses (8.0%)
C, Q      : 4 wins, 48 losses (7.7%)
M, S      : 3 wins, 42 losses (6.7%)
C, O      : 3 wins, 48 losses (5.9%)
D, O      : 2 wins, 36 losses (5.3%)
D, Q      : 2 wins, 36 losses (5.3%)
Q, S      : 2 wins, 40 losses (4.8%)
M, Q      : 2 wins, 40 losses (4.8%)
D, R      : 2 wins, 49 losses (3.9%)
O, R      : 1 wins, 32 losses (3.0%)
C, M      : 1 wins, 43 losses (2.3%)
D, S      : 1 wins, 46 losses (2.1%)
C, D      : 0 wins, 47 losses (0.0%)
C, R      : 0 wins, 43 losses (0.0%)

## Alternate implementations
- https://github.com/alexzherdev/pandemic

# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
    - alternate: https://docs.google.com/viewer?a=v&pid=sites&srcid=ZGVmYXVsdGRvbWFpbnxzcGlsbGVyZWdsfGd4OjQ2YTMzM2E1NDg4ZGQwNzE

# todo
- test randomness: occasionally tests will fail due to some kind of game randomness
- improve win rates on heroic difficulty
    - greedy agent can win ~1/1000 random 2 player heroic games :)
    - idea: try DFS with greedy move selector
    - idea: variable-depth (greedy?) lookahead
    - pandemic strategies
        - easily win on 6, no strat mentioned: https://www.reddit.com/r/boardgames/comments/7zk0dr/how_difficult_is_it_to_win_pandemic_with_6/
        - indicates 6 is possible, some strats: https://boardgamegeek.com/thread/2356305/questions-pandemic-base-game-heroic-mode
            - clear cubes early game, while building hands
            - more players: easier to clear cubes, harder to cure. Inverse for fewer players
        - read more:
            - BGA has replays! stats here: https://forum.boardgamearena.com/viewtopic.php?t=25373
            - https://diceboardcards.wordpress.com/2013/08/16/how-to-win-pandemic-on-hard-mode-heroic-a-review/
            - https://boardgames.stackexchange.com/questions/2372/what-are-good-general-strategies-for-pandemic
- make pandemic game correct by construction? make all properties get-only
    - hide command and event handlers if not hidden already. pandemic public api should make sense
      in terms of game rules, no internal details
