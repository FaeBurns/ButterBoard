using ButterBoard.Building;
using Newtonsoft.Json;

namespace ButterBoard.SaveSystem
{
    public class SavedBuildAction
    {
        [JsonProperty]
        public int NextRegisterIdAtStart { get; }
        
        [JsonProperty]
        public BuildAction Action { get; }

        public SavedBuildAction(int nextRegisterIdAtStart, BuildAction action)
        {
            NextRegisterIdAtStart = nextRegisterIdAtStart;
            Action = action;
        }
    }
}