using System;
using System.Collections.Generic;
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
                GridPoint? gridPoint = GetPoint();

                if (gridPoint == null)
                    return;

                IReadOnlyCollection<Wire> localWires = GameManager.Instance.ConnectionManager.GetLocalConnections(gridPoint.Wire);
                IReadOnlyCollection<Wire> globalWires = GameManager.Instance.ConnectionManager.GetGlobalConnections(gridPoint.Wire);

                Debug.Log($"[{gridPoint.gameObject.name}] {(gridPoint.Open ? "Open" : "Closed")} | {(gridPoint.Blocked ? "Blocked" : "Free")} | {(gridPoint.ConnectedPin != null ? "Connected" : "Disconnected")} | {localWires.Count} Local Connections | {globalWires.Count} Global Connections");
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