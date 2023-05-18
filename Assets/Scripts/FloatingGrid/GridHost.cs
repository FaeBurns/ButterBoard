using System;
using System.Collections.Generic;
using ButterBoard.Building;
using ButterBoard.FloatingGrid.Placement.Placeables;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridHost : MonoBehaviour
    {
        public void Initialize(int width, int height, float spacing, Vector3 offset, List<GridPoint> gridPoints, List<GridPointConnectedRow> connectedRows)
        {
            Width = width;
            Height = height;
            Spacing = spacing;
            TopLeftOffsetFromCenter = offset;
            GridPoints = gridPoints;
            ConnectedRows = connectedRows;
        }

        [field: SerializeField]
        public int Key { get; set; } = -1;

        [field: SerializeField]
        public int Width { get; private set; }

        [field: SerializeField]
        public int Height { get; private set; }

        [field: SerializeField]
        public float Spacing { get; private set; }

        [field: SerializeField]
        public bool CablesOnly { get; private set; } = false;

        [field: SerializeField]
        public Vector3 TopLeftOffsetFromCenter { get; private set; }

        [field: SerializeField]
        public List<GridPoint> GridPoints { get; private set; } = null!;

        [field: SerializeField]
        public List<GridPointConnectedRow> ConnectedRows { get; private set; } = null!;

        [field: SerializeField]
        public List<GameObject> SnapPoints { get; private set; } = null!;

        public Transform AttachedPlaceablesHostTransform { get; private set; } = null!;

        private void Awake()
        {
            AttachedPlaceablesHostTransform = new GameObject().transform;
            AttachedPlaceablesHostTransform.SetParent(transform);
            AttachedPlaceablesHostTransform.transform.position = new Vector3(0, 0, -1);
        }

        private void OnDestroy()
        {
            if (Key != -1)
                BuildManager.RemoveRegisteredGridHost(Key);
        }
    }
}