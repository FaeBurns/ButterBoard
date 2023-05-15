using System;
using System.Collections.Generic;
using ButterBoard.Simulation;
using Coil;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPointScanner : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                try
                {
                    GridPoint? gridPoint = GetPoint();

                    if (gridPoint == null)
                        return;

                    IReadOnlyCollection<Wire> localWires = SimulationManager.Instance.ConnectionManager.GetLocalConnections(gridPoint.Wire);
                    IReadOnlyCollection<Wire> globalWires = SimulationManager.Instance.ConnectionManager.GetGlobalConnections(gridPoint.Wire);

                    string poweredStatus = (PowerManager.GetHasPower(gridPoint) ? "Powered" : "Unpowered");
                    string powerSourceStatus = (PowerManager.GetIsProvidingPower(gridPoint) ? "Source" : "Not Source");

                    Debug.Log($"[{gridPoint.gameObject.name}] {(gridPoint.Open ? "Open" : "Closed")} | {(gridPoint.Blocked ? "Blocked" : "Free")} | {(gridPoint.ConnectedPin != null ? "Connected" : "Disconnected")} | {localWires.Count} Local Connections | {globalWires.Count} Global Connections | {poweredStatus} | {powerSourceStatus}");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        private GridPoint? GetPoint()
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(PlacementHelpers.GetMouseWorldPosition(), 0.1f);
            foreach (Collider2D overlap in overlaps)
            {
                GridPoint? point = overlap.GetComponent<GridPoint>();
                if (point != null)
                    return point;
            }
            return null;
        }
    }
}