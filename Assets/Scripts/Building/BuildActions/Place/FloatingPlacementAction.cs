using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Place
{
    public class FloatingPlacementAction : BuildAction
    {
        [JsonProperty]
        private string _prefabKey;
        [JsonProperty]
        private Vector2 _location;
        [JsonProperty]
        private float _rotation;
        
        [JsonIgnore]
        private int _recordedKey;

        public FloatingPlacementAction(FloatingPlaceable placeable, Vector2 location, float rotation)
        {
            _prefabKey = placeable.SourceAssetKey;
            _location = location;
            _rotation = rotation;
            _recordedKey = placeable.Key;
        }
        
        public override void Execute()
        {
            FloatingPlaceable spawnedPlaceable = FloatingBuildHandler.Place(_prefabKey, _location, _rotation);
            _recordedKey = BuildManager.RegisterPlaceable(spawnedPlaceable, BuildManager.GetNextOrExistingId(_recordedKey));
        }

        public override void UndoExecute()
        {
            FloatingBuildHandler.Remove(BuildManager.GetPlaceable<FloatingPlaceable>(_recordedKey));
            BuildManager.RemoveRegisteredPlaceable(_recordedKey);
        }
    }
}