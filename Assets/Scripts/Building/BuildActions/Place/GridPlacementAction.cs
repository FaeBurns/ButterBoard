using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Place
{
    public class GridPlacementAction : BuildAction
    {
        [JsonProperty]
        private string _prefabKey;
        
        [JsonProperty]
        private Vector2 _targetPosition;
        
        [JsonProperty]
        private float _targetRotation;
        
        [JsonProperty]
        private int[] _connectingPointIndices;
        [JsonProperty]
        private int[] _blockingPointIndices;

        [JsonProperty]
        private int _targetGridKey;

        [JsonIgnore]
        private int _recordedKey;

        public GridPlacementAction(GridPlaceable placeable, Vector2 location, float rotation, int targetGridKey, int[] connectingPointIndices, int[] blockingPointIndices)
        {
            _prefabKey = placeable.SourceAssetKey;
            _targetPosition = location;
            _targetRotation = rotation;
            _targetGridKey = targetGridKey;
            _recordedKey = placeable.Key;

            _connectingPointIndices = connectingPointIndices;
            _blockingPointIndices = blockingPointIndices;
        }

        public override void Execute()
        {
            GridPlaceable placeable = GridBuildHandler.Place(_prefabKey, _targetPosition, _targetRotation, _targetGridKey, _connectingPointIndices, _blockingPointIndices);
            _recordedKey = BuildManager.RegisterPlaceable(placeable, BuildManager.GetNextOrExistingId(_recordedKey));
        }

        public override void UndoExecute()
        {
            GridBuildHandler.Remove(BuildManager.GetPlaceable<GridPlaceable>(_recordedKey));
            BuildManager.RemoveRegisteredPlaceable(_recordedKey);
        }
    }
}