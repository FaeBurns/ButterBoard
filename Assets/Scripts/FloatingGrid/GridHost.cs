using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridHost : MonoBehaviour
    {
        public void Initialize(int width, int height, float spacing, Vector3 offset, List<GridPoint> gridPoints)
        {
            Width = width;
            Height = height;
            Spacing = spacing;
            TopLeftOffsetFromCenter = offset;
            GridPoints = gridPoints;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public float Spacing { get; private set; }

        public Vector3 TopLeftOffsetFromCenter { get; private set; }

        public IReadOnlyList<GridPoint> GridPoints { get; private set; } = null!;
    }
}