using System.Collections.Generic;
using ButterBoard.Cables;
using Coil;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPointConnectedRow : MonoBehaviour
    {
        [field: SerializeField]
        public List<GridPoint> GridPoints { get; private set; } = null!;

        [SerializeField]
        public GridLineIndicator? lineIndicator;

        public void Initialize(List<GridPoint> gridPoints)
        {
            GridPoints = gridPoints;
        }

        private void Awake()
        {
            Wire wire = new Wire();

            // set GridPoints to all use same wire
            foreach (GridPoint gridPoint in GridPoints)
            {
                gridPoint.Wire = wire;
            }

            if (lineIndicator != null)
                lineIndicator.Initialize(wire);
        }
    }
}