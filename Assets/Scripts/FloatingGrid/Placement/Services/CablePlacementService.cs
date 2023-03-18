using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.Cables;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Lookup;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class CablePlacementService : PlacementService<CablePlaceable>
    {
        private readonly float _pinCheckDistanceRadiusThreshold;
        private GameObject? _prefab;
        private CablePlaceable? _startPlaceable;
        private CablePlacementType _placementType;

        public CablePlacementService(LerpSettings lerpSettings, float pinCheckDistanceRadiusThreshold, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
            _pinCheckDistanceRadiusThreshold = pinCheckDistanceRadiusThreshold;
        }

        public override void BeginPrefabPlacement(GameObject prefab)
        {
            base.BeginPrefabPlacement(prefab);
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
            GameManager.Instance.ConnectionManager.Disconnect(Context.Placeable.Other.Pin.ConnectedPoint!.Wire, Context.Placeable.Pin.ConnectedPoint!.Wire);

            Context.Placeable.Pin.ConnectedPoint!.Free();
            Context.Placeable.Pin.Free();
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
            Context.Placeable.Pin.Connect(targetPoint);
            targetPoint.Connect(Context.Placeable.Pin);

            // perform connection of cable placeables if currently valid
            if (_placementType == CablePlacementType.END && _startPlaceable != null)
            {
                _startPlaceable.Other = Context.Placeable;
                Context.Placeable.Other = _startPlaceable;
            }

            // perform connection in Coil if this is the end side
            // does not need to be full cable placement for connection to be required
            if (_placementType == CablePlacementType.END)
            {
                // connect the two wires together
                // but only if they are not the same wire
                if (Context.Placeable.Pin.ConnectedPoint!.Wire != Context.Placeable.Other.Pin.ConnectedPoint!.Wire)
                    GameManager.Instance.ConnectionManager.Connect(Context.Placeable.Pin.ConnectedPoint!.Wire, Context.Placeable.Other.Pin.ConnectedPoint!.Wire);
            }

            // set parent to target point's grid
            Context.PlacingObject.transform.SetParent(targetPoint.HostingGrid.transform);

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
                base.BeginPrefabPlacement(_prefab!);

                // force into end state
                _placementType = CablePlacementType.END;
                Context.Placeable.PlacementType = _placementType;

                // connect placeables
                Context.Placeable.Other = _startPlaceable!;
                _startPlaceable!.Other = Context.Placeable;

                // invoke place on the first
                _startPlaceable!.Place?.Invoke();

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
            CablePlaceable cablePlaceable = (CablePlaceable)target;

            // destroy display line
            if (cablePlaceable.LineDisplay != null)
                Object.Destroy(cablePlaceable.LineDisplay.gameObject);

            // if an other exists
            // remove it too
            // should only not occur if Remove is called during CancelPlacement
            if (cablePlaceable.Other != null)
            {
                // check that the wires are not the same and that there is a local connection from target to its other
                if (cablePlaceable.Pin.ConnectedPoint!.Wire != cablePlaceable.Other.Pin.ConnectedPoint!.Wire &&
                    GameManager.Instance.ConnectionManager.GetLocalConnections(cablePlaceable.Pin.ConnectedPoint!.Wire).Contains(cablePlaceable.Other.Pin.ConnectedPoint!.Wire))
                {
                    GameManager.Instance.ConnectionManager.Disconnect(cablePlaceable.Other.Pin.ConnectedPoint!.Wire, cablePlaceable.Pin.ConnectedPoint!.Wire);
                }

                Object.Destroy(cablePlaceable.Other.gameObject);
            }

            base.Remove(target);
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