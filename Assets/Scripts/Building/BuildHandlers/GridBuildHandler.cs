using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Lookup;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.Building.BuildHandlers
{
    public static class GridBuildHandler
    {
        public static GridPlaceable Place(string prefabKey, Vector2 position, float rotation, int gridHostPlaceableId, int[] connectingPointIndices, int[] blockingPointIndices)
        {
            // create prefab
            GameObject prefab = AssetSource.Fetch<GameObject>(prefabKey)!;

            // get grid to connect to
            GridHost hostGrid = BuildManager.GetPlaceable(gridHostPlaceableId).GetComponentInChildren<GridHost>();

            // spawn placeable at position and get GridPlaceable component from it
            GridPlaceable placeable = Object.Instantiate(prefab, position, Quaternion.Euler(0, 0, rotation), hostGrid.transform).GetComponent<GridPlaceable>();
            placeable.SourceAssetKey = prefabKey;

            Lock(placeable, hostGrid, connectingPointIndices, blockingPointIndices);
            
            // hide all PinIdentifierDisplays
            foreach (PinIdentifierDisplay pinIdentifier in placeable.GetComponentsInChildren<PinIdentifierDisplay>())
            {
                pinIdentifier.gameObject.SetActive(false);
            }
            
            // notify of placement
            placeable.Place?.Invoke();

            // record rotation
            placeable.PlacedRotation = rotation;

            return placeable;
        }

        public static void Remove(GridPlaceable target)
        {
            target.Remove.Invoke();
            
            Unlock(target);
            
            Object.Destroy(target.gameObject);
        }

        public static void Move(GridPlaceable target, Vector2 targetPosition, float targetRotation, int targetGridId, int[] connectingPointIndices, int[] blockingPointIndices)
        {
            Unlock(target);

            BasePlaceable gridPlaceable = BuildManager.GetPlaceable(targetGridId);
            GridHost gridHost = gridPlaceable.GetComponentInChildren<GridHost>();

            target.transform.SetPositionAndRotation(targetPosition, Quaternion.Euler(0, 0, targetRotation));
            
            gridPlaceable.PlacedRotation = targetRotation;
            
            Lock(target, gridHost, connectingPointIndices, blockingPointIndices);
        }

        private static void Lock(GridPlaceable placeable, GridHost targetGrid, int[] connectingPointIndices, int[] blockingPointIndices)
        {
            // get size of placeable
            Vector2 size = placeable.GetSize();

            // get all GridPoints being blocked, set them in blocking points and block them
            placeable.BlockingPoints = new GridPoint[blockingPointIndices.Length];
            for (int i = 0; i < blockingPointIndices.Length; i++)
            {
                placeable.BlockingPoints[i] = targetGrid.GridPoints[i];
                targetGrid.GridPoints[i].Blocked = true;
            }

            Debug.Assert(connectingPointIndices.Length == placeable.Pins.Count, "connectingPointIndices.Length == placeable.Pins.Count");
            
            // connect points
            for (int i = 0; i < connectingPointIndices.Length; i++)
            {
                int pointIndex = connectingPointIndices[i];
                BuildManager.Connect(placeable.Pins[i], targetGrid.GridPoints[pointIndex]);
            }
            
            placeable.HostingGrid = targetGrid;
            placeable.transform.SetParent(targetGrid.transform);
        }

        private static void Unlock(GridPlaceable placeable)
        {
            foreach (GridPin pin in placeable.Pins)
            {
                BuildManager.RemoveConnections(pin);
            }

            foreach (GridPoint point in placeable.BlockingPoints)
            {
                point.Blocked = false;
            }

            placeable.HostingGrid = null;
        }
    }
}