namespace pandemic.console;

using System.Collections.Generic;

internal class GameStats
{
    /// <summary>
    /// {num actions : count}
    /// </summary>
    public readonly Dictionary<int, int> ActionsPerGameCounts = [];

    /// <summary>
    /// {num legal actions : count}
    /// </summary>
    public readonly Dictionary<int, int> LegalActionCounts = [];

    public void AddNumActionsInGame(int numActions)
    {
        ActionsPerGameCounts[numActions] = ActionsPerGameCounts.TryGetValue(
            numActions,
            out var value
        )
            ? ++value
            : 1;
    }

    public void AddLegalActionCount(int numLegalActions)
    {
        LegalActionCounts[numLegalActions] = LegalActionCounts.TryGetValue(
            numLegalActions,
            out var value
        )
            ? ++value
            : 1;
    }
}
