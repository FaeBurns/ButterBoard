﻿using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.Building;
using ButterBoard.Building.BuildActions.Move;
using ButterBoard.Building.BuildActions.Place;
using ButterBoard.Building.BuildActions.Remove;
using ButterBoard.Cables;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Lookup;
using UnityEngine;
using ButterBoard.Simulation;
using ButterBoard.UI.Rack;
using Object = UnityEngine.Object;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class CablePlacementService : PlacementService<CablePlaceable>
    {
        private readonly float _pinCheckDistanceRadiusThreshold;
        private GameObject? _prefab;
        private CablePlaceable? _startPlaceable;
        private CablePlacementType _placementType;
        private GridHost _moveOriginalHost = null!;
        private int _moveOriginalPointIndex = -1;

        public CablePlacementService(LerpSettings lerpSettings, float pinCheckDistanceRadiusThreshold, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
            _pinCheckDistanceRadiusThreshold = pinCheckDistanceRadiusThreshold;
        }

        public override void BeginPrefabPlacement(GameObject prefab, string assetSourceKey)
        {
            base.BeginPrefabPlacement(prefab, assetSourceKey);
            _prefab = prefab;
            _placementType = CablePlacementType.START;
            _startPlaceable = Context.Placeable;

            Context.Placeable.PlacementType = _placementType;
        }

        public override void BeginMovePlacement(GameObject target)
        {
            base.BeginMovePlacement(target);
            _prefab = null;
            _placementType = CablePlacementType.END;

            // disconnect the two wires of the cable being moved
            if (Context.Placeable.Pin.ConnectedPoint.Wire != Context.Placeable.OtherCable.Pin.ConnectedPoint.Wire)
                SimulationManager.Instance.ConnectionManager.Disconnect(Context.Placeable.OtherCable.Pin.ConnectedPoint.Wire, Context.Placeable.Pin.ConnectedPoint.Wire);

            _moveOriginalHost = Context.Placeable.Pin.ConnectedPoint.HostingGrid;
            _moveOriginalPointIndex = Context.Placeable.Pin.ConnectedPoint.PointIndex;
            
            BuildManager.RemoveConnections(Context.Placeable.Pin);
        }

        protected override bool CommitPlacement()
        {
            // get target grid point
            GridPoint? targetPoint = GetGridPointAtPosition(Context.CheckingObject.transform.position);

            // cancel if not over grid point
            if (targetPoint == null)
                return false;

            // cancel if not open
            if (!targetPoint.Open)
                return false;

            // perform connection
            BuildManager.Connect(Context.Placeable.Pin, targetPoint);

            // perform connection of cable placeables if currently valid
            if (_placementType == CablePlacementType.END && _startPlaceable != null)
            {
                _startPlaceable.OtherCable = Context.Placeable;
                Context.Placeable.OtherCable = _startPlaceable;
            }

            // perform connection in Coil if this is the end side
            // does not need to be full cable placement for connection to be required
            if (_placementType == CablePlacementType.END)
            {
                // connect the two wires together
                // but only if they are not the same wire
                if (Context.Placeable.Pin.ConnectedPoint.Wire != Context.Placeable.OtherCable.Pin.ConnectedPoint.Wire)
                    SimulationManager.Instance.ConnectionManager.Connect(Context.Placeable.Pin.ConnectedPoint.Wire, Context.Placeable.OtherCable.Pin.ConnectedPoint.Wire);
            }

            // set parent to target point's grid
            Context.PlacingObject.transform.SetParent(targetPoint.HostingGrid.transform);

            // notify limiter of placement
            if (Context.PlacementType == PlacementType.PLACE && _placementType == CablePlacementType.END)
                PlacementLimitManager.MarkPlacement(Context.Placeable);

            if (_placementType == CablePlacementType.END)
            {
                BuildAction action;
                switch (Context.PlacementType)
                {
                    case PlacementType.PLACE:
                        BuildManager.RegisterPlaceable(Context.Placeable, BuildManager.GetNextRegistryId());
                        BuildManager.RegisterPlaceable(Context.Placeable.OtherCable, BuildManager.GetNextRegistryId());
                        action = new CablePlacementAction(Context.Placeable);
                        break;
                    case PlacementType.MOVE:
                        action = new CableMoveAction(Context.Placeable, _moveOriginalPointIndex, targetPoint.PointIndex, _moveOriginalHost, targetPoint.HostingGrid);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                
                BuildActionManager.Instance.PushNoExecuteAction(action);
            }

            // notify of success
            return true;
        }

        protected override void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            // get grid under target
            GridPoint? targetPoint = GetGridPointAtPosition(targetPosition);

            // if no target found
            if (targetPoint == null)
            {
                // set unsnapped position
                SetPositionAndRotation(targetPosition, targetRotation);

                // display error
                Context.Placeable.DisplayPlacementStatus("Must be placed on a grid", false);
                return;
            }

            // snap position to grid
            Vector3 snappedPosition = PlacementHelpers.SnapPositionToGrid(targetPoint.HostingGrid, targetPosition, Vector3.zero);
            SetPositionAndRotation(snappedPosition, targetRotation);

            // if targeted point is not open
            if (!targetPoint.Open)
            {
                // display error
                Context.Placeable.DisplayPlacementStatus("Grid point is in use", false);
                return;
            }

            // display okay
            Context.Placeable.DisplayPlacementStatus(String.Empty, true);
        }

        protected override bool UpdateFinalize()
        {
            bool baseResult = base.UpdateFinalize();

            // exit if finalize has not yet finished
            if (!baseResult)
                return false;

            // finalize has now finished
            // if this is placing the start of the cable
            if (_placementType == CablePlacementType.START)
            {
                // destroy start display object
                Object.Destroy(Context.CheckingObject);

                // reset display status
                Context.Placeable.SetDisplayStatus(false);

                // begin placement of the cable end point
                // IMPORTANT - calling base here
                // otherwise _startPlaceable gets set
                // and icky stuff happens
                base.BeginPrefabPlacement(_prefab!, Context.Placeable.SourceAssetKey);

                // force into end state
                _placementType = CablePlacementType.END;
                Context.Placeable.PlacementType = _placementType;

                // connect placeables
                Context.Placeable.OtherCable = _startPlaceable!;
                _startPlaceable!.OtherCable = Context.Placeable;

                // invoke place on the first
                _startPlaceable!.Place.Invoke();

                // create cable display
                GameObject cableDisplayObject = (GameObject)Object.Instantiate(AssetSource.Fetch("Objects/CableDisplay"));
                CableDisplay cableDisplay = cableDisplayObject.GetComponent<CableDisplay>();

                // set cable targets
                cableDisplay.Initialize(_startPlaceable, Context.Placeable);
                _startPlaceable.LineDisplay = cableDisplay;
                Context.Placeable.LineDisplay = cableDisplay;

                // return false - execution of UpdatePosition will now continue
                return false;
            }

            // otherwise placement has concluded
            return true;
        }

        public override void Remove(BasePlaceable target)
        {
            CableRemoveAction removeAction = new CableRemoveAction(target.Key);
            BuildActionManager.Instance.PushAndExecuteAction(removeAction);

            // CablePlaceable cablePlaceable = (CablePlaceable)target;
            //
            // // destroy display line
            // if (cablePlaceable.LineDisplay != null)
            //     Object.Destroy(cablePlaceable.LineDisplay.gameObject);
            //
            // // if an other exists
            // // remove it too
            // // should only be false if Remove is called during CancelPlacement
            // if (cablePlaceable.OtherCable != null)
            // {
            //     // check if this placeable has a connected point - will be false if canceled during the second half of placement
            //     if (cablePlaceable.Pin.ConnectedPoint != null)
            //     {
            //         // check that the wires are not the same and that there is a local connection from target to its other
            //         if (cablePlaceable.Pin.ConnectedPoint.Wire != cablePlaceable.OtherCable.Pin.ConnectedPoint.Wire &&
            //             SimulationManager.Instance.ConnectionManager.GetLocalConnections(cablePlaceable.Pin.ConnectedPoint.Wire).Contains(cablePlaceable.OtherCable.Pin.ConnectedPoint.Wire))
            //         {
            //             SimulationManager.Instance.ConnectionManager.Disconnect(cablePlaceable.OtherCable.Pin.ConnectedPoint.Wire, cablePlaceable.Pin.ConnectedPoint.Wire);
            //         }
            //     }
            //
            //     Object.Destroy(cablePlaceable.OtherCable.gameObject);
            // }
            //
            // base.Remove(target);
        }

        private GridPoint? GetGridPointAtPosition(Vector3 position)
        {
            List<GridPoint> points = GetOverlapsCircle<GridPoint>(position, _pinCheckDistanceRadiusThreshold + Context.Size.x);

            // if nothing was found, return 0
            if (points.Count == 0)
                return null;

            // otherwise return the first
            return points[0];
        }
    }

    public enum CablePlacementType
    {
        /// <summary>
        /// Placing the first pin.
        /// </summary>
        START,

        /// <summary>
        /// Placing the second pin.
        /// </summary>
        END,
    }
}