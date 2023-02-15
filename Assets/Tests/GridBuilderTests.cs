using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.FloatingGrid;
using ButterBoard.Lookup;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GridBuilderTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void GridBuilderTestsSimpleBuildTopLeft()
        {
            const int width = 10;
            const int height = 5;
            const float spacing = 1;

            GameObject gridBuilderObject = new GameObject();
            GridBuilder gridBuilder = gridBuilderObject.AddComponent<GridBuilder>();

            gridBuilder.Build(null, width, height, spacing, GridBuildOffsetType.TOP_LEFT);

            Assert.AreEqual(50, gridBuilder.ActivePoints.Count);

            List<Vector3> desiredPositions = new List<Vector3>();
            List<Vector3> actualPositions = gridBuilder.ActivePoints.Select(p => p.transform.position).ToList();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    desiredPositions.Add(new Vector3(x * spacing, y * spacing, 0));
                }
            }

            Assert.AreEqual(desiredPositions, actualPositions);
        }

        public void GridBuilderTestsSimpleBuildCenter()
        {
            const int width = 10;
            const int height = 5;
            const float spacing = 1;

            GameObject gridBuilderObject = new GameObject();
            GridBuilder gridBuilder = gridBuilderObject.AddComponent<GridBuilder>();

            gridBuilder.Build(null, width, height, spacing, GridBuildOffsetType.CENTER);

            Assert.AreEqual(50, gridBuilder.ActivePoints.Count);

            List<Vector3> desiredPositions = new List<Vector3>();
            List<Vector3> actualPositions = gridBuilder.ActivePoints.Select(p => p.transform.position).ToList();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    desiredPositions.Add(new Vector3(x * spacing, y * spacing, 0));
                }
            }

            Assert.AreEqual(desiredPositions, actualPositions);
        }
    }
}
