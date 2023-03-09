using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPointConnectedRow : MonoBehaviour
    {
        [field: SerializeField]
        public List<GridPoint> GridPoints { get; private set; } = null!;

        public void Initialize(List<GridPoint> gridPoints)
        {
            GridPoints = gridPoints;
        }
    }
}