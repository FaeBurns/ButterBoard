using System.Collections.Generic;
using System.Linq;

namespace ButterBoard.Building.SaveSystem
{
    public class SaveData
    {
        public BuildAction[] SavedBuildActions { get; }
        
        public SaveData(IEnumerable<BuildAction> savedBuildActions)
        {
            SavedBuildActions = savedBuildActions.ToArray();
        }
    }
}