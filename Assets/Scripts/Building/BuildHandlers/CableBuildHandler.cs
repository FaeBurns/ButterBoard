using System;
using ButterBoard.Cables;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Lookup;
using ButterBoard.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.Building.BuildHandlers
{
    public static class CableBuildHandler
    {
        /// <summary>
        /// Places a cable between two positions. Assumes all positions are valid.
        /// </summary>
        /// <param name="prefabKey">The prefab key used to create the cable.</param>
        /// <param name="pointIndexA">The index of the <see cref="GridPoint"/> end A of the cable should connect to.</param>
        /// <param name="pointIndexB">The index of the <see cref="GridPoint"/> end B of the cable should connect to.</param>
        /// <param name="gridHostAKey">The key of <paramref name="pointIndexA"/>'s grid's placeable.</param>
        /// <param name="gridHostBKey">The key of <paramref name="pointIndexB"/>'s grid's placeable.</param>
        /// <returns>The cable end created at <paramref name="pointIndexA"/>.</returns>
        /// <exception cref="InvalidOperationException">Could not find an open point at <paramref name="pointIndexA"/> or <paramref name="pointIndexB"/>.</exception>
        public static CablePlaceable Place(string prefabKey, int pointIndexA, int pointIndexB, int gridHostAKey, int gridHostBKey)
        {
            // get the GridPoints that each cable end will connect to
            GridPoint pointA = GetGridPoint(pointIndexA, gridHostAKey);
            GridPoint pointB = GetGridPoint(pointIndexB, gridHostBKey);
            
            // get prefabs for cable and cable display
            GameObject prefab = AssetSource.Fetch<GameObject>(prefabKey)!;
            GameObject displayPrefab = AssetSource.Fetch<GameObject>("Objects/CableDisplay")!;
            
            // create cable pin objects
            CablePlaceable cableA = Object.Instantiate(prefab, pointA.transform.position, Quaternion.identity, pointA.HostingGrid.transform).GetComponent<CablePlaceable>();
            CablePlaceable cableB = Object.Instantiate(prefab, pointB.transform.position, Quaternion.identity, pointB.HostingGrid.transform).GetComponent<CablePlaceable>();
            
            // save prefab source key
            cableA.SourceAssetKey = prefabKey;
            cableB.SourceAssetKey = prefabKey;

            // create display object and initialize with the two cable ends
            CableDisplay cableDisplay = Object.Instantiate(displayPrefab).GetComponent<CableDisplay>();
            cableDisplay.Initialize(cableA, cableB);

            // make connection between each cable end
            cableA.OtherCable = cableB;
            cableB.OtherCable = cableA;
            
            // keep track of line display on each cable end
            cableA.LineDisplay = cableDisplay;
            cableB.LineDisplay = cableDisplay;

            // connect pins to points
            BuildManager.Connect(cableA.Pin, pointA);
            BuildManager.Connect(cableB.Pin, pointB);
            
            // create bridge between wires, but only if they are not the same wire
            if (pointA.Wire != pointB.Wire)
                SimulationManager.Instance.ConnectionManager.Connect(pointA.Wire, pointB.Wire);
            
            // invoke placement events
            cableA.Place.Invoke();
            cableA.Place.Invoke();

            // return the start placeable
            // does not matter which one is returned
            return cableA;
        }

        /// <summary>
        /// Removes a cable at both ends along with its <see cref="CableDisplay"/>.
        /// </summary>
        /// <param name="target"></param>
        public static void Remove(CablePlaceable target)
        {
            // get other end of cable
            CablePlaceable otherCable = target.OtherCable;
            
            // invoke removal events on both
            target.Remove.Invoke();
            otherCable.Remove.Invoke();

            // remove bridge between wires, but only if they are not the same wire
            if (target.Pin.ConnectedPoint.Wire != otherCable.Pin.ConnectedPoint.Wire)
                SimulationManager.Instance.ConnectionManager.Disconnect(target.Pin.ConnectedPoint.Wire, otherCable.Pin.ConnectedPoint.Wire);
            
            // remove connections between pins and points
            BuildManager.RemoveConnections(target.Pin);
            BuildManager.RemoveConnections(otherCable.Pin);
            
            // destroy all cable objects
            Object.Destroy(target.LineDisplay.gameObject);
            Object.Destroy(otherCable.gameObject);
            Object.Destroy(target.gameObject);
        }

        /// <summary>
        /// Moves a <see cref="CablePlaceable"/> into a new position. Expects the movement to be valid.
        /// </summary>
        /// <param name="target">The <see cref="CablePlaceable"/> to move.</param>
        /// <param name="targetPointId">The index of the <see cref="GridPoint"/> to move the cable end to.</param>
        /// <param name="gridHostId">The id of the <see cref="GridHost"/> the cable end is being moved to.</param>
        /// <exception cref="InvalidOperationException">No valid open <see cref="GridPoint"/> was found at the destination position.</exception>
        public static void Move(CablePlaceable target, int targetPointId, int gridHostId)
        {
            CablePlaceable otherCable = target.OtherCable;
            
            // disconnect wire bridge
            if (target.Pin.ConnectedPoint.Wire != otherCable.Pin.ConnectedPoint.Wire)
                SimulationManager.Instance.ConnectionManager.Disconnect(target.Pin.ConnectedPoint.Wire, otherCable.Pin.ConnectedPoint.Wire);
            
            // remove any existing connections
            BuildManager.RemoveConnections(target.Pin);

            // try and get point it's being connected to
            GridPoint point = GetGridPoint(targetPointId, gridHostId);
            
            // set to new position
            target.transform.position = point.transform.position;
            
            // perform connection
            BuildManager.Connect(target.Pin, point);
            
            // recreate bridge between new connections
            if (target.Pin.ConnectedPoint.Wire != otherCable.Pin.ConnectedPoint.Wire)
                SimulationManager.Instance.ConnectionManager.Connect(target.Pin.ConnectedPoint.Wire, otherCable.Pin.ConnectedPoint.Wire);
        }
        
        public static GridPoint GetGridPoint(int gridPointIndex, int hostingGridId)
        {
            GridHost hostingGrid = BuildManager.GetRegisteredGridHost(hostingGridId);
            return hostingGrid.GridPoints[gridPointIndex];
        }
    }
}