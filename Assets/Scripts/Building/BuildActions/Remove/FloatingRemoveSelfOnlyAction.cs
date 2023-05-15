using System;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Remove
{
    public class FloatingRemoveSelfOnlyAction : BuildAction
    {
        [JsonProperty] private int _placeableKey;

        [JsonProperty] private string _sourceAssetKey = String.Empty;
        [JsonProperty] private Vector2 _location;
        [JsonProperty] private float _rotation;
        
        public static FloatingRemoveSelfOnlyAction CreateInstance(int placeableKey)
        {
            FloatingPlaceable placeable = BuildManager.GetPlaceable<FloatingPlaceable>(placeableKey);
            Transform placeableTransform = placeable.transform;

            return new FloatingRemoveSelfOnlyAction()
            {
                _placeableKey = placeableKey,

                _sourceAssetKey = placeable.SourceAssetKey,
                _location = placeableTransform.position,
                _rotation = placeableTransform.rotation.eulerAngles.z,
            };
        }

        public override void Execute()
        {
            FloatingBuildHandler.Remove(BuildManager.GetPlaceable<FloatingPlaceable>(_placeableKey));
            BuildManager.RemoveRegisteredPlaceable(_placeableKey);
        }

        public override void UndoExecute()
        {
            FloatingPlaceable placeable = FloatingBuildHandler.Place(_sourceAssetKey, _location, _rotation);
            BuildManager.RegisterPlaceable(placeable, _placeableKey);
        }
    }
}