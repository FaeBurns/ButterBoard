namespace ButterBoard.SaveSystem
{
    /// <summary>
    /// Save data for a component on a placeable object.
    /// </summary>
    public abstract class PlaceableSaveData
    {
        /// <summary>
        /// The key of the placeable this save data belongs to.
        /// </summary>
        public int PlaceableKey { get; set; }
    }
}