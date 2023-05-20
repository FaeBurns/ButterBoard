namespace ButterBoard.SaveSystem
{
    /// <summary>
    /// Interface for save and load events on a placeable object.
    /// </summary>
    public interface ISaveLoadListener
    {
        public PlaceableSaveData Save();
        public void Load(PlaceableSaveData data);   
    }
}