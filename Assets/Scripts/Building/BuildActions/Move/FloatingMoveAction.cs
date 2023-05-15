using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Move
{
    public class FloatingMoveAction : BuildAction
    {
        [JsonProperty]
        private int _placeableKey;
        [JsonProperty]
        private Vector2 _targetPosition;
        [JsonProperty]
        private Vector2 _sourcePosition;
        [JsonProperty]
        private float _targetRotation;
        [JsonProperty]
        private float _sourceRotation;

        public FloatingMoveAction(FloatingPlaceable placeable, Vector2 sourcePosition, float sourceRotation, Vector2 targetPosition, float targetRotation)
        {
            _placeableKey = placeable.Key;
            _sourcePosition = sourcePosition;
            _sourceRotation = sourceRotation;

            _targetPosition = targetPosition;
            _targetRotation = targetRotation;
        }

        public override void Execute()
        {
            FloatingBuildHandler.Move(BuildManager.GetPlaceable<FloatingPlaceable>(_placeableKey), _targetPosition, _targetRotation);
        }

        public override void UndoExecute()
        {
            FloatingBuildHandler.Move(BuildManager.GetPlaceable<FloatingPlaceable>(_placeableKey), _sourcePosition, _sourceRotation);
        }
    }
}