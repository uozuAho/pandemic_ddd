using System.Collections.Generic;

namespace pandemic.console;

class GameStats
{
    /// <summary>
    /// {num actions : count}
    /// </summary>
    public readonly Dictionary<int, int> ActionsPerGameCounts = new();

    /// <summary>
    /// {num legal actions : count}
    /// </summary>
    public readonly Dictionary<int, int> LegalActionCounts = new();

    public void AddNumActionsInGame(int numActions)
    {
        if (ActionsPerGameCounts.ContainsKey(numActions))
            ActionsPerGameCounts[numActions]++;
        else
            ActionsPerGameCounts[numActions] = 1;
    }

    public void AddLegalActionCount(int numLegalActions)
    {
        if (LegalActionCounts.ContainsKey(numLegalActions))
            LegalActionCounts[numLegalActions]++;
        else
            LegalActionCounts[numLegalActions] = 1;
    }
}
