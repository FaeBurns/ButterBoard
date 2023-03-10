using System.Collections.Generic;
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
        public int Width { get; private set; }

        [field: SerializeField]
        public int Height { get; private set; }

        [field: SerializeField]
        public float Spacing { get; private set; }

        [field: SerializeField]
        public Vector3 TopLeftOffsetFromCenter { get; private set; }

        [field: SerializeField]
        public List<GridPoint> GridPoints { get; private set; } = null!;

        [field: SerializeField]
        public List<GridPointConnectedRow> ConnectedRows { get; private set; } = null!;

        [field: SerializeField]
        public List<GameObject> SnapPoints { get; private set; } = null!;
    }
}