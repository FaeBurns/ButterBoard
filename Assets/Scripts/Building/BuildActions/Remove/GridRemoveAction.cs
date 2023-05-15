using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Remove
{
    public class GridRemoveAction : BuildAction
    {
        [JsonProperty]
        private int _placeableKey;

        [JsonIgnore]
        private readonly string _sourceAssetKey;
        [JsonIgnore]
        private readonly Vector2 _location;
        [JsonIgnore]
        private readonly float _rotation;
        [JsonIgnore]
        private readonly int _gridHostId;
        [JsonIgnore]
        private readonly int[] _connectingPointIndices;
        [JsonIgnore]
        private readonly int[] _blockingPointIndices;

        public GridRemoveAction(int placeableKey, int gridHostId)
        {
            _placeableKey = placeableKey;

            GridPlaceable placeable = BuildManager.GetPlaceable<GridPlaceable>(placeableKey);
            Transform placeableTransform = placeable.transform;

            _sourceAssetKey = placeable.SourceAssetKey;
            _location = placeableTransform.position;
            _rotation = placeableTransform.rotation.eulerAngles.z;
            _gridHostId = gridHostId;

            _connectingPointIndices = new int[placeable.Pins.Count];
            for (int i = 0; i < placeable.Pins.Count; i++)
            {
                _connectingPointIndices[i] = placeable.Pins[i].ConnectedPoint.PointIndex;
            }

            _blockingPointIndices = new int[placeable.BlockingPoints.Length];
            for (int i = 0; i < placeable.BlockingPoints.Length; i++)
            {
                _blockingPointIndices[i] = placeable.BlockingPoints[i].PointIndex;
            }
        }

        public override void Execute()
        {
            GridBuildHandler.Remove(BuildManager.GetPlaceable<GridPlaceable>(_placeableKey));
            BuildManager.RemoveRegisteredPlaceable(_placeableKey);
        }

        public override void UndoExecute()
        {
            GridPlaceable placeable = GridBuildHandler.Place(_sourceAssetKey, _location, _rotation, _gridHostId, _connectingPointIndices, _blockingPointIndices);
            BuildManager.RegisterPlaceable(placeable, _placeableKey);
        }
    }
}