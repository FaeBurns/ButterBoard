using System;
using System.Collections.Generic;
using Coil;
using Coil.Connections;
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

        private void Awake()
        {
            Wire wire = new Wire(new SynchronizedValueSource());

            foreach (GridPoint gridPoint in GridPoints)
            {
                gridPoint.Wire = wire;
            }
        }
    }
}