using System;
using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
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

        public IReadOnlyList<GridPoint> ActivePoints => _activePoints;

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

        public void Build(Transform? parent, int width, int height, float spacing, GridBuildOffsetType offsetType)
        {
            Debug.Log($"Building grid with width {width}, height {height}, spacing {spacing}, offsetType {offsetType}");
            GameObject pointPrefab = AssetSource.Fetch<GameObject>(gridPointPrefabName);

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
                    xOffset = (width / 2f) * -spacing;
                    yOffset = (height / 2f) * -spacing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(offsetType), offsetType, null);
            }

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
                    GameObject newObject = Instantiate(pointPrefab, position, Quaternion.identity, parent);
                    GridPoint newGridPoint = newObject.GetComponent<GridPoint>();

                    _activePoints.Add(newGridPoint);
                }
            }
        }
    }
}