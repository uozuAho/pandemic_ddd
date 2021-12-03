# Pandemic

The [Pandemic board game](https://en.wikipedia.org/wiki/Pandemic_%28board_game%29),
implemented in C#. Intended for usage by AI agents.

Work in progress! A full game is playable, but many game rules are yet to be
implemented.

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

# todo
- mcts agent
    - copy all mcts code DONE
    - copy mcts tests DONE
    - mcts speedups
        - clean up pandemic.console
        - don't go forever! try alpha beta or something
        - make a note of the mutable2 branch. Mutability gives 4x speedup
          in situations where less cloning is needed, eg. MCTS rollout
    - refactor
        - address todos
        - address warnings
        - remove null forgiving operators
- implement epidemic
- implement more game rules
- check code todos

# References
- [Game rules](https://www.ultraboardgames.com/pandemic/game-rules.php)
