using System;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Remove
{
    public class GridRemoveAction : BuildAction
    {
        [JsonProperty] private int _placeableKey;

        [JsonProperty] private string _sourceAssetKey = String.Empty;
        [JsonProperty] private Vector2 _location;
        [JsonProperty] private float _rotation;
        [JsonProperty] private int _gridHostId;
        [JsonProperty] private int[] _connectingPointIndices = Array.Empty<int>();
        [JsonProperty] private int[] _blockingPointIndices = Array.Empty<int>();

        public static GridRemoveAction CreateInstance(int placeableKey, int gridHostId)
        {
            GridPlaceable placeable = BuildManager.GetPlaceable<GridPlaceable>(placeableKey);
            Transform placeableTransform = placeable.transform;
            
            int[] connectingPointIndices = new int[placeable.Pins.Count];
            for (int i = 0; i < placeable.Pins.Count; i++)
            {
                connectingPointIndices[i] = placeable.Pins[i].ConnectedPoint.PointIndex;
            }

            int[] blockingPointIndices = new int[placeable.BlockingPoints.Length];
            for (int i = 0; i < placeable.BlockingPoints.Length; i++)
            {
                blockingPointIndices[i] = placeable.BlockingPoints[i].PointIndex;
            }

            return new GridRemoveAction()
            {
                _placeableKey = placeableKey,

                _sourceAssetKey = placeable.SourceAssetKey,
                _location = placeableTransform.position,
                _rotation = placeableTransform.rotation.eulerAngles.z,
                _gridHostId = gridHostId,
                
                _connectingPointIndices = connectingPointIndices,
                _blockingPointIndices = blockingPointIndices,
            };
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