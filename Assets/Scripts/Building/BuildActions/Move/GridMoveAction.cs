using System.ComponentModel;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Move
{
    [DisplayName("Move")]
    public class GridMoveAction : BuildAction
    {
        [JsonProperty] private int _placeableKey;
        [JsonProperty] private Vector2 _targetPosition;
        [JsonProperty] private float _targetRotation;
        [JsonProperty] private int _targetGridId;
        [JsonProperty] private int[] _targetConnectingPointIndices = null!;
        [JsonProperty] private int[] _targetBlockingPointIndices = null!;

        [JsonProperty] private Vector2 _sourcePosition;
        [JsonProperty] private float _sourceRotation;
        [JsonProperty] private int _sourceGridId;
        [JsonProperty] private int[] _sourceConnectingPointIndices = null!;
        [JsonProperty] private int[] _sourceBlockingPointIndices = null!;

        public static GridMoveAction CreateInstance(GridPlaceable placeable, Vector2 sourcePosition, float sourceRotation, Vector2 targetPosition, float targetRotation, int targetGridId, int[] targetConnectingPointIndices, int[] targetBlockingPointIndices)
        {
            int[] sourceConnectingPointIndices = new int[placeable.Pins.Count];
            for (int i = 0; i < placeable.Pins.Count; i++)
            {
                sourceConnectingPointIndices[i] = placeable.Pins[i].ConnectedPoint.PointIndex;
            }

            int[] sourceBlockingPointIndices = new int[placeable.BlockingPoints.Length];
            for (int i = 0; i < placeable.BlockingPoints.Length; i++)
            {
                sourceBlockingPointIndices[i] = placeable.BlockingPoints[i].PointIndex;
            }

            return new GridMoveAction()
            {
                _placeableKey = placeable.Key,

                _sourcePosition = sourcePosition,
                _targetPosition = targetPosition,

                _sourceRotation = sourceRotation,
                _targetRotation = targetRotation,

                _targetGridId = targetGridId,
                _sourceGridId = placeable.HostingGrid.Key,

                _targetConnectingPointIndices = targetConnectingPointIndices,
                _targetBlockingPointIndices = targetBlockingPointIndices,

                _sourceConnectingPointIndices = sourceConnectingPointIndices,
                _sourceBlockingPointIndices = sourceBlockingPointIndices,
            };
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