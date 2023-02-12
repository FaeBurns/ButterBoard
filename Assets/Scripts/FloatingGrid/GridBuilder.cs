using System;
using System.Collections.Generic;
using ButterBoard.Lookup;
using JetBrains.Annotations;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GridBuilder : MonoBehaviour
    {
        private List<GridPoint> _activePoints = new List<GridPoint>();

        [SerializeField]
        private string gridPointPrefabName = "Grid_Point";

        public void Clear()
        {
            foreach (GridPoint point in _activePoints)
            {
                if (point.ConnectedPin != null)
                    point.ConnectedPin.Free();
                point.Free();

                if (Application.isPlaying)
                    Destroy(point.gameObject);
                else
                    DestroyImmediate(point.gameObject);
            }
            _activePoints.Clear();
        }

        public void Build(Transform parent, int width, int height, float spacing, GridBuildOffsetType offsetType)
        {
            Debug.Log($"Building grid with width {width}, height {height}, spacing {spacing}, offsetType {offsetType}");
            GameObject pointPrefab = PrefabSource.Fetch(gridPointPrefabName);

            // go y-x instead of x-y so that it runs horizontally first
            // loop vertical
            for (int y = 0; y < height; y++)
            {
                // loop horizontal
                for (int x = 0; x < width; x++)
                {
                    // spawn new point at position
                    // scale by spacing to set distance between points
                    Vector3 position = new Vector3(x * spacing, y * spacing, 0);
                    GameObject newGridPoint = Instantiate(pointPrefab, position, Quaternion.identity, parent);

                    _activePoints.Add(newGridPoint.GetComponent<GridPoint>());
                }
            }
        }
    }
}