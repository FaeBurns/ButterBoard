using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Remove
{
    public class FloatingRemoveSelfOnlyAction : BuildAction
    {
        [JsonProperty]
        private int _placeableKey;

        [JsonIgnore]
        private readonly string _sourceAssetKey;
        [JsonIgnore]
        private readonly Vector2 _location;
        [JsonIgnore]
        private readonly float _rotation;
        
        public FloatingRemoveSelfOnlyAction(int placeableKey)
        {
            _placeableKey = placeableKey;

            FloatingPlaceable placeable = BuildManager.GetPlaceable<FloatingPlaceable>(placeableKey);
            Transform placeableTransform = placeable.transform;

            _sourceAssetKey = placeable.SourceAssetKey;
            _location = placeableTransform.position;
            _rotation = placeableTransform.rotation.eulerAngles.z;
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