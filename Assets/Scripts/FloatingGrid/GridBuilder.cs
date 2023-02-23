﻿using System;
using System.Collections.Generic;
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

        public void Clear(GridHost clearingHost)
        {
            foreach (GridPoint point in clearingHost.GridPoints)
            {
                // check for if already destroyed
                if (point == null)
                    continue;

                // free on both ends if possible
                if (point.ConnectedPin != null)
                    point.ConnectedPin.Free();
                point.Free();

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
            GameObject pointPrefab = AssetSource.Fetch<GameObject>(gridPointPrefabName);


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
            Vector3 offset = new Vector3(xOffset, yOffset, 0);

            gridHost.Initialize(width, height, spacing, offset, allPoints);

            // go y-x instead of x-y so that it runs horizontally first
            // loop vertical
            for (int y = 0; y < height; y++)
            {
                // loop horizontal
                for (int x = 0; x < width; x++)
                {
                    // spawn new point at position
                    // scale by spacing to set distance between points
                    // add offsets to modify position if placing at center of parent
                    Vector3 position = new Vector3((x * spacing) + xOffset, (y * spacing) + yOffset, 0);

                    GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(pointPrefab, gridHost.transform);
                    newObject.transform.position = position;
                    GridPoint newGridPoint = newObject.GetComponent<GridPoint>();

                    allPoints.Add(newGridPoint);
                }
            }

            foreach (GridPoint gridPoint in gridHost.GridPoints)
            {
                gridPoint.Initialize(gridHost);
            }

            return gridHost;
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
}