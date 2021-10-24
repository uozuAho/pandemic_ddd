namespace pandemic.client
{
    internal interface ISpielGame
    {
        /// <summary>
        /// OpenSpiel game's GetType method. Renamed since GetType is
        /// already implemented on C# objects.
        /// </summary>
        int GetGameType();
        int MaxUtility();
    }
}
