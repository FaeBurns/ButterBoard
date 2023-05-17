using System;
using System.Collections.Generic;
using ButterBoard.Building;
using ButterBoard.Lookup;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField]
        private string gridPointPrefabName = "FloatingGrid/Grid_Point";

        [SerializeField]
        private GridBuildRowDirection rowDirection;

        public void Clear(GridHost clearingHost)
        {
            foreach (GridPoint point in clearingHost.GridPoints)
            {
                // check for if already destroyed
                if (point == null)
                    continue;

                BuildManager.RemoveConnections(point);

                // destroy - different versions required if in edit or play mode
                if (Application.isPlaying)
                    Destroy(point.gameObject);
                else
                    DestroyImmediate(point.gameObject);
            }

            if (Application.isPlaying)
                Destroy(clearingHost.gameObject);
            else
                DestroyImmediate(clearingHost.gameObject);
        }

        public GridHost Build(Transform? parent, int width, int height, float spacing, GridBuildOffsetType offsetType)
        {
            Debug.Log($"Building grid with width {width}, height {height}, spacing {spacing}, offsetType {offsetType}");
            GameObject pointPrefab = AssetSource.Fetch<GameObject>(gridPointPrefabName)!;

            GridHost gridHost = new GameObject("New Grid", typeof(GridHost)).GetComponent<GridHost>();

            // set parent of GridHost if set
            if (parent != null)
                gridHost.transform.SetParent(parent);

            // get offsets to use during placement
            float xOffset;
            float yOffset;
            switch (offsetType)
            {
                // no offset as placing in top left
                case GridBuildOffsetType.TOP_LEFT:
                    xOffset = 0;
                    yOffset = 0;
                    break;

                // get offset to place points in center
                case GridBuildOffsetType.CENTER:
                    Vector2 gridCenter = GetGridCenter(width, height, spacing);
                    xOffset = -gridCenter.x;
                    yOffset = -gridCenter.y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(offsetType), offsetType, null);
            }

            List<GridPoint> allPoints = new List<GridPoint>();
            List<GridPointConnectedRow> connectedRows = new List<GridPointConnectedRow>();
            Vector3 offset = new Vector3(xOffset, yOffset, 0);

            int index = 0;

            switch (rowDirection)
            {
                case GridBuildRowDirection.VERTICAL:
                    CreateGridVerticalRow(width, height, spacing, xOffset, yOffset, gridHost, index, pointPrefab, allPoints, connectedRows);
                    break;
                case GridBuildRowDirection.HORIZONTAL:
                    CreateGridHorizontalRow(width, height, spacing, xOffset, yOffset, gridHost, index, pointPrefab, allPoints, connectedRows);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            gridHost.Initialize(width, height, spacing, offset, allPoints, connectedRows);

            foreach (GridPoint gridPoint in gridHost.GridPoints)
            {
                gridPoint.Initialize(gridHost);
            }

            return gridHost;
        }

        private static void CreateGridVerticalRow(int width, int height, float spacing, float xOffset, float yOffset, GridHost gridHost, int index, GameObject pointPrefab, List<GridPoint> allPoints, List<GridPointConnectedRow> connectedRows)
        {
            // loop horizontal
            for (int x = 0; x < width; x++)
            {
                GridPointConnectedRow connectedRow = new GameObject("Row_" + x).AddComponent<GridPointConnectedRow>();
                connectedRow.transform.position = new Vector3((x * spacing) + xOffset, 0, 0);
                connectedRow.transform.SetParent(gridHost.transform);
                List<GridPoint> rowPoints = new List<GridPoint>();

                // loop vertical
                for (int y = 0; y < height; y++)
                {
                    index = SpawnPoint(spacing, x, xOffset, y, yOffset, pointPrefab, connectedRow, index, allPoints, rowPoints);
                    index++;
                }

                connectedRow.Initialize(rowPoints);
                connectedRows.Add(connectedRow);
            }
        }

        private static void CreateGridHorizontalRow(int width, int height, float spacing, float xOffset, float yOffset, GridHost gridHost, int index, GameObject pointPrefab, List<GridPoint> allPoints, List<GridPointConnectedRow> connectedRows)
        {
            // loop vertical
            for (int y = 0; y < height; y++)
            {
                GridPointConnectedRow connectedRow = new GameObject("Row_" + y).AddComponent<GridPointConnectedRow>();
                connectedRow.transform.position = new Vector3(0, (y * spacing) + yOffset, 0);
                connectedRow.transform.SetParent(gridHost.transform);
                List<GridPoint> rowPoints = new List<GridPoint>();

                // loop horizontal
                for (int x = 0; x < width; x++)
                {
                    index = SpawnPoint(spacing, x, xOffset, y, yOffset, pointPrefab, connectedRow, index, allPoints, rowPoints);
                    index++;
                }

                connectedRow.Initialize(rowPoints);
                connectedRows.Add(connectedRow);
            }
        }

        private static int SpawnPoint(float spacing, int x, float xOffset, int y, float yOffset, GameObject pointPrefab, GridPointConnectedRow connectedRow, int index, List<GridPoint> allPoints, List<GridPoint> rowPoints)
        {

            // spawn new point at position
            // scale by spacing to set distance between points
            // add offsets to modify position if placing at center of parent
            Vector3 position = new Vector3((x * spacing) + xOffset, (y * spacing) + yOffset, 0);

#if UNITY_EDITOR
            // instantiate using PrefabUtility to keep prefab link
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(pointPrefab, connectedRow.transform);
#else
            // PrefabUtility is not available in builds and this script cannot belong to the editor library - this code should never run but added just to be safe
            GameObject newObject = (GameObject)Instantiate(pointPrefab, connectedRow.transform);
#endif
            newObject.transform.position = position;
            GridPoint newGridPoint = newObject.GetComponent<GridPoint>();
            newGridPoint.PointIndex = index;

            allPoints.Add(newGridPoint);
            rowPoints.Add(newGridPoint);
            return index;
        }

        public static Vector2 GetGridCenter(int width, int height, float spacing)
        {
            // xOffset = ((width - 1) * spacing) / 2f;
            // yOffset = ((height - 1) * spacing) / 2f;

            float x = ((width - 1) * spacing) / 2f;
            float y = ((height - 1) * spacing) / 2f;

            return new Vector2(x, y);
        }
    }

    [Serializable]
    public enum GridBuildRowDirection
    {
        VERTICAL,
        HORIZONTAL,
    }
}