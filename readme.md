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
- pandemic: core game logic. Immutable & DDD-like.
- pandemic.agents: agents that can play games
- pandemic.console: scratchpad console app. Uncomment stuff in Program.cs to run it.
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

## Agent performance
Uncomment `WinLossStats.PlayGamesAndPrintWinLossStats` to get these stats. So far,
the best performance I've got is from playing many games with `GreedyAgent`s. This
agent picks the 'best' move based on a `GameEvaluator`, which is just a scorer I
wrote to indicate how 'good' a game state is.

Other agents like greedy best-first, DFS run forever without finding a win. I think
the app needs some performance improvements to make search-based agents more viable.
Also, I think there is quite a bit of luck involved in winning a game, so searching
is futile for unwinnable games.

## Alternate implementations
- https://github.com/alexzherdev/pandemic

# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
    - alternate: https://docs.google.com/viewer?a=v&pid=sites&srcid=ZGVmYXVsdGRvbWFpbnxzcGlsbGVyZWdsfGd4OjQ2YTMzM2E1NDg4ZGQwNzE

# todo
- improve win rates on heroic difficulty
    - greedy agent can win ~1/1000 random 2 player heroic games :)
    - try DFS with greedy move selector
    - make 'hasenoughtocure' work for scientist/researcher (whoever cures with 4)
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
