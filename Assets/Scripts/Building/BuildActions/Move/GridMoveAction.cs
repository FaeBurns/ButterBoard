using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Move
{
    public class GridMoveAction : BuildAction
    {
        [JsonProperty] private int _placeableKey;
        [JsonProperty] private Vector2 _targetPosition;
        [JsonProperty] private float _targetRotation;
        [JsonProperty] private int _targetGridId;
        [JsonProperty] private int[] _targetConnectingPointIndices;
        [JsonProperty] private int[] _targetBlockingPointIndices;

        [JsonIgnore] private readonly int _sourceGridId;
        [JsonIgnore] private readonly Vector2 _sourcePosition;
        [JsonIgnore] private readonly float _sourceRotation;
        [JsonIgnore] private readonly int[] _sourceConnectingPointIndices;
        [JsonIgnore] private readonly int[] _sourceBlockingPointIndices;

        public GridMoveAction(GridPlaceable placeable, Vector2 sourcePosition, float sourceRotation, Vector2 targetPosition, float targetRotation, int targetGridId, int[] targetConnectingPointIndices, int[] targetBlockingPointIndices)
        {
            _placeableKey = placeable.Key;

            _sourcePosition = sourcePosition;
            _targetPosition = targetPosition;
            
            _sourceRotation = sourceRotation;
            _targetRotation = targetRotation;

            _targetGridId = targetGridId;
            _sourceGridId = placeable.HostingGrid!.GetComponentInParent<BasePlaceable>().Key;

            _targetConnectingPointIndices = targetConnectingPointIndices;
            _targetBlockingPointIndices = targetBlockingPointIndices;
            
            _sourceConnectingPointIndices = new int[placeable.Pins.Count];
            for (int i = 0; i < placeable.Pins.Count; i++)
            {
                _sourceConnectingPointIndices[i] = placeable.Pins[i].ConnectedPoint.PointIndex;
            }

            _sourceBlockingPointIndices = new int[placeable.BlockingPoints.Length];
            for (int i = 0; i < placeable.BlockingPoints.Length; i++)
            {
                _sourceBlockingPointIndices[i] = placeable.BlockingPoints[i].PointIndex;
            }
        }

        public override void Execute()
        {
            GridBuildHandler.Move(BuildManager.GetPlaceable<GridPlaceable>(_placeableKey), _targetPosition, _targetRotation, _targetGridId, _targetConnectingPointIndices, _targetBlockingPointIndices);
        }

        public override void UndoExecute()
        {
            GridBuildHandler.Move(BuildManager.GetPlaceable<GridPlaceable>(_placeableKey), _sourcePosition, _sourceRotation, _sourceGridId, _sourceConnectingPointIndices, _sourceBlockingPointIndices);
        }
    }
}