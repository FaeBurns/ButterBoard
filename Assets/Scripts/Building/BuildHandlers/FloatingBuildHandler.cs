using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Lookup;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.Building.BuildHandlers
{
    public static class FloatingBuildHandler
    {
        public static FloatingPlaceable Place(string prefabKey, Vector2 position, float rotation)
        {
            GameObject prefab = AssetSource.Fetch<GameObject>(prefabKey)!;

            GameObject spawnedObject = Object.Instantiate(prefab, position, Quaternion.Euler(0, 0, rotation), BuildManager.GetFloatingPlaceableHost());

            FloatingPlaceable placeable = spawnedObject.GetComponent<FloatingPlaceable>();

            placeable.SourceAssetKey = prefabKey;
            placeable.Place?.Invoke();

            placeable.PlacedRotation = rotation;

            return placeable;
        }

        public static void Move(FloatingPlaceable target, Vector2 targetPosition, float targetRotation)
        {
            target.transform.SetPositionAndRotation(targetPosition, Quaternion.Euler(0, 0, targetRotation));
            target.PlacedRotation = targetRotation;
        }

        public static void Remove(FloatingPlaceable target)
        {
            // assumes there are no children
            target.Remove?.Invoke();

            BuildManager.RemoveRegisteredPlaceable(target.Key);

            Object.Destroy(target.gameObject);
        }
    }
}