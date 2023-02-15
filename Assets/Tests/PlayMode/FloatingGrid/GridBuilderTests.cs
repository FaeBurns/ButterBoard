using System.Collections.Generic;
using System.Linq;
using ButterBoard.FloatingGrid;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode.FloatingGrid
{
    public class GridBuilderTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void GridBuilderTestsSimpleBuildTopLeft()
        {
            const int width = 5;
            const int height = 2;
            const float spacing = 1;

            GameObject gridBuilderObject = new GameObject();
            GridBuilder gridBuilder = gridBuilderObject.AddComponent<GridBuilder>();

            GridHost gridHost = gridBuilder.Build(gridBuilderObject.transform, width, height, spacing, GridBuildOffsetType.TOP_LEFT);

            Assert.AreEqual(width * height, gridHost.GridPoints.Count);

            List<Vector2> actualPositions = gridHost.GridPoints.Select(p => p.transform.position).Select(v => (Vector2)v).ToList();

            List<Vector2> desiredPositions = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(2, 0),
                new Vector2(3, 0),
                new Vector2(4, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(2, 1),
                new Vector2(3, 1),
                new Vector2(4, 1),
            };

            Assert.AreEqual(desiredPositions, actualPositions);
        }

        [Test]
        public void GridBuilderTestsSimpleBuildCenter()
        {
            const int width = 4;
            const int height = 2;
            const float spacing = 1;

            GameObject gridBuilderObject = new GameObject();
            GridBuilder gridBuilder = gridBuilderObject.AddComponent<GridBuilder>();

            GridHost gridHost = gridBuilder.Build(gridBuilderObject.transform, width, height, spacing, GridBuildOffsetType.CENTER);

            Assert.AreEqual(width * height, gridHost.GridPoints.Count);

            List<Vector2> actualPositions = gridHost.GridPoints.Select(p => p.transform.position).Select(v => (Vector2)v).ToList();

            List<Vector2> desiredPositions = new List<Vector2>
            {
                new Vector2(-1.5f, -0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(1.5f, -0.5f),
                new Vector2(-1.5f, 0.5f),
                new Vector2(-0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(1.5f, 0.5f),
            };

            Assert.AreEqual(desiredPositions, actualPositions);
        }

        [Test]
        public void GridBuilderTestsGetGridCenter()
        {
            Assert.AreEqual(new Vector2(2, 1), GridBuilder.GetGridCenter(5, 3, 1f));
            Assert.AreEqual(new Vector2(1.5f, 0.5f), GridBuilder.GetGridCenter(4, 2, 1f));

            Assert.AreEqual(new Vector2(1, 0.5f), GridBuilder.GetGridCenter(5, 3, 0.5f));
            Assert.AreEqual(new Vector2(0.75f, 0.25f), GridBuilder.GetGridCenter(4, 2, 0.5f));

            Assert.AreEqual(new Vector2(4, 2), GridBuilder.GetGridCenter(5, 3, 2f));
            Assert.AreEqual(new Vector2(3f, 1f), GridBuilder.GetGridCenter(4, 2, 2f));

            Assert.AreEqual(new Vector2(0, 0.5f), GridBuilder.GetGridCenter(1, 2, 1f));
        }

        [Test]
        public void GridBuilderTestHostSet()
        {
            const int width = 4;
            const int height = 2;
            const float spacing = 1;

            GameObject gridBuilderObject = new GameObject();
            GridBuilder gridBuilder = gridBuilderObject.AddComponent<GridBuilder>();

            GridHost gridHost = gridBuilder.Build(gridBuilderObject.transform, width, height, spacing, GridBuildOffsetType.TOP_LEFT);

            foreach (GridPoint gridPoint in gridHost.GridPoints)
            {
                Assert.IsTrue(gridPoint.HostGridHost == gridHost);
            }
        }
    }
}
