using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.FloatingGrid.Placement.Placeables;
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

                    if (gridPoint != null)
                    {
                        IReadOnlyCollection<Wire> localWires = SimulationManager.Instance.ConnectionManager.GetLocalConnections(gridPoint.Wire);
                        IReadOnlyCollection<Wire> globalWires = SimulationManager.Instance.ConnectionManager.GetGlobalConnections(gridPoint.Wire);

                        string poweredStatus = (PowerManager.GetHasPower(gridPoint) ? "Powered" : "Unpowered");
                        string powerSourceStatus = (PowerManager.GetIsProvidingPower(gridPoint) ? "Source" : "Not Source");

                        Debug.Log(
                            $"[{gridPoint.gameObject.name}] {(gridPoint.Open ? "Open" : "Closed")} | {(gridPoint.Blocked ? "Blocked" : "Free")} | {(gridPoint.ConnectedPin != null ? "Connected" : "Disconnected")} | {localWires.Count} Local Connections | {globalWires.Count} Global Connections | {poweredStatus} | {powerSourceStatus}");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                try
                {
                    BasePlaceable[] placeables = GetPlaceable().ToArray();
                    foreach (BasePlaceable placeable in placeables)
                    {
                        switch (placeable)
                        {
                            case CablePlaceable cablePlaceable:
                                Debug.Log($"Cable. Connected grid: {cablePlaceable.Pin.ConnectedPoint.HostingGrid.Key}, Connected FloatingPlaceable: {cablePlaceable.Pin.ConnectedPoint.HostingGrid.GetComponentInParent<BasePlaceable>().Key}");
                                break;
                            case FloatingPlaceable floatingPlaceable:
                                Debug.Log($"Floating: {floatingPlaceable.Key}");
                                break;
                            case GridPlaceable gridPlaceable:
                                Debug.Log($"Grid. Connected grid: {gridPlaceable.Pins[0].ConnectedPoint.HostingGrid.Key}, Connected FloatingPlaceable: {gridPlaceable.Pins[0].ConnectedPoint.HostingGrid.GetComponentInParent<BasePlaceable>().Key}");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(placeable));
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    throw;
                }
                {

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

        private IEnumerable<BasePlaceable> GetPlaceable()
        {
            Collider2D[] overlaps = Physics2D.OverlapPointAll(PlacementHelpers.GetMouseWorldPosition());
            foreach (Collider2D overlap in overlaps)
            {
                BasePlaceable? point = overlap.GetComponent<BasePlaceable>();
                if (point != null)
                    yield return point;
            }
        }
    }
}