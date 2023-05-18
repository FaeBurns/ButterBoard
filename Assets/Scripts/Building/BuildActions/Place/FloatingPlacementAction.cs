using System;
using System.ComponentModel;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Place
{
    [DisplayName("Place")]
    public class FloatingPlacementAction : BuildAction
    {
        [JsonProperty] private string _prefabKey = String.Empty;
        [JsonProperty] private Vector2 _location;
        [JsonProperty] private float _rotation;
        [JsonProperty] private int _recordedKey = -1;
        [JsonProperty] private int[] _gridKeys = Array.Empty<int>();

        public static FloatingPlacementAction CreateInstance(FloatingPlaceable placeable, Vector2 location, float rotation)
        {
            return new FloatingPlacementAction()
            {
                _prefabKey = placeable.SourceAssetKey,
                _location = location,
                _rotation = rotation,
                _recordedKey = placeable.Key,
            };
        }

        public override void Execute()
        {
            FloatingPlaceable spawnedPlaceable = FloatingBuildHandler.Place(_prefabKey, _location, _rotation);
            _recordedKey = BuildManager.RegisterPlaceable(spawnedPlaceable, BuildManager.GetNextOrExistingId(_recordedKey));
            _gridKeys = BuildManager.RegisterFloatingGrids(spawnedPlaceable, _gridKeys);
        }

        public override void UndoExecute()
        {
            FloatingBuildHandler.Remove(BuildManager.GetPlaceable<FloatingPlaceable>(_recordedKey));
            BuildManager.RemoveRegisteredPlaceable(_recordedKey);
        }
    }
}