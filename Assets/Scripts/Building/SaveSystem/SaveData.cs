using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.SaveSystem
{
    public class SaveData
    {
        [JsonProperty]
        public BuildAction[] SavedBuildActions { get; set; }
        
        [JsonProperty]
        public PlaceableSaveData[] ObjectSaveData { get; set; }
        
        [JsonProperty]
        public Vector2 CameraPosition { get; set; }
        
        [JsonProperty]
        public float CameraZoom { get; set; }
        
        public SaveData(IEnumerable<BuildAction> savedBuildActions, IEnumerable<PlaceableSaveData> objectSaveData)
        {
            SavedBuildActions = savedBuildActions.ToArray();
            ObjectSaveData = objectSaveData.ToArray();
        }
    }
}