using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.Building;
using ButterBoard.Building.BuildActions.Move;
using ButterBoard.Building.BuildActions.Place;
using ButterBoard.Building.BuildActions.Remove;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.UI.Rack;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class GridPlacementService : PlacementService<GridPlaceable>
    {
        private readonly float _pinCheckDistanceRadiusThreshold;
        private readonly Dictionary<GridPin, GridPin> _checkingToRealGridPinMapping = new Dictionary<GridPin, GridPin>();

        private GridHost? _previousGridHost;

        public GridPlacementService(LerpSettings lerpSettings, float pinCheckDistanceRadiusThreshold, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
            _pinCheckDistanceRadiusThreshold = pinCheckDistanceRadiusThreshold;
        }

        public override void BeginPrefabPlacement(GameObject prefab, string assetSourceKey)
        {
            base.BeginPrefabPlacement(prefab, assetSourceKey);

            for (int i = 0; i < Context.CheckingPlaceable.Pins.Count; i++)
            {
                _checkingToRealGridPinMapping.Add(Context.CheckingPlaceable.Pins[i], Context.Placeable.Pins[i]);
            }
        }

        /// <inheritdoc/>
        public override void BeginMovePlacement(GameObject target)
        {
            base.BeginMovePlacement(target);

            // disconnect all GridPins
            foreach (GridPin gridPin in Context.Placeable.Pins)
            {
                BuildManager.RemoveConnections(gridPin);
            }

            for (int i = 0; i < Context.Placeable.Pins.Count; i++)
            {
                _checkingToRealGridPinMapping.Add(Context.CheckingPlaceable.Pins[i], Context.Placeable.Pins[i]);
            }

            // unblock all points
            foreach (GridPoint point in Context.Placeable.BlockingPoints)
            {
                point.Blocked = false;
            }

            // show all PinIdentifierDisplays
            foreach (PinIdentifierDisplay pinIdentifier in target.GetComponentsInChildren<PinIdentifierDisplay>(true))
            {
                pinIdentifier.gameObject.SetActive(true);
            }
        }

        public override void Remove(BasePlaceable target)
        {
            GridPlaceable placeable = (GridPlaceable)target;
            GridRemoveAction removeAction = GridRemoveAction.CreateInstance(target.Key, placeable.HostingGrid.Key);
            BuildActionManager.Instance.PushAndExecuteAction(removeAction);
        }

        protected override bool CommitPlacement()
        {
            GridHost? gridTarget = GetTargetGrid(Context.Placeable.transform.position);

            if (gridTarget == null)
                return false;

            bool isValid = GetPinIssues(gridTarget, Context.CheckingPlaceable).Count == 0;

            if (!isValid)
                return false;

            // get list of all overlapping points
            List<GridPoint> overlappingPoints = GetOverlaps<GridPoint>(Context.CheckingPlaceable);

            // check if any of those points are occupied - cannot be placed over closed ports that aren't on the pins
            isValid = true;
            foreach (GridPoint gridPoint in overlappingPoints)
            {
                if (!gridPoint.Open)
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                return false;

            // get mapping of pins to points
            Dictionary<GridPin,GridPoint> pinTargets = GetPointsUnderPins(gridTarget, Context.CheckingPlaceable);

            // get set of all points targeted by pins
            HashSet<GridPoint> pinPoints = new HashSet<GridPoint>(pinTargets.Values);

            List<int> blockingPointIndices = new List<int>();
            
            // set points covered by bounds of placeable to blocked
            // but only those not getting connected to pins
            foreach (GridPoint gridPoint in overlappingPoints)
            {
                // skip if found in pinPoints
                if (pinPoints.Contains(gridPoint))
                    continue;

                // mark point as blocked
                gridPoint.Blocked = true;
                
                blockingPointIndices.Add(gridPoint.PointIndex);
            }

            int[] connectingPointIndices = new int[pinTargets.Count];
            
            // connect all pins to points
            int i = 0;
            foreach ((GridPin checkingPin, GridPoint gridPoint) in pinTargets)
            {
                GridPin targetPin = _checkingToRealGridPinMapping[checkingPin];

                BuildManager.Connect(targetPin, gridPoint);
                
                // record the index of the point being connected to - required for actions
                connectingPointIndices[i] = gridPoint.PointIndex;
                i++;
            }

            Context.Placeable.HostingGrid = gridTarget;

            Context.Placeable.BlockingPoints = overlappingPoints.ToArray();

            Context.Placeable.ClearPlacementStatus();
            Context.PlacingObject.transform.SetParent(gridTarget.transform);

            // hide all PinIdentifierDisplays
            foreach (PinIdentifierDisplay pinIdentifier in Context.Placeable.GetComponentsInChildren<PinIdentifierDisplay>())
            {
                pinIdentifier.gameObject.SetActive(false);
            }

            int gridKey = gridTarget.Key;
            
            BuildAction action; 
            switch (Context.PlacementType)
            {
                case PlacementType.PLACE:
                    BuildManager.RegisterPlaceable(Context.Placeable, BuildManager.GetNextRegistryId());
                    action = GridPlacementAction.CreateInstance(Context.Placeable, Context.CheckingObject.transform.position, Context.CheckingObject.transform.rotation.eulerAngles.z, gridKey, connectingPointIndices, blockingPointIndices.ToArray());
                    break;
                case PlacementType.MOVE:
                    action = GridMoveAction.CreateInstance(Context.Placeable, moveInitialPosition, moveInitialRotation, Context.CheckingObject.transform.position, Context.CheckingObject.transform.rotation.eulerAngles.z, gridKey, connectingPointIndices, blockingPointIndices.ToArray());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            BuildActionManager.Instance.PushNoExecuteAction(action);
                
            return true;
        }

        protected override void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            // clear pin issues
            // probably don't need to do this every frame
            foreach (GridPin pin in Context.Placeable.Pins)
            {
                pin.ClearIssue();
            }

            // get all grids that the placeable would be overlapping at the target position
            GridHost? targetGrid = GetTargetGrid(targetPosition);

            // if there were no overlapping grids found
            if (targetGrid == null)
            {
                // set position and rotation to the raw values and return
                SetPositionAndRotation(targetPosition, targetRotation);

                // notify user of error
                Context.Placeable.DisplayPlacementStatus("Must be placed on a grid", false);
                return;
            }

            Vector3 snappedPosition;

            // multiplying quaternions adds them together in the order of multiplication
            Quaternion snappedRotation = targetRotation * targetGrid.transform.rotation;

            if (Context.Placeable.SnapsToGridSnapPoints && targetGrid.SnapPoints.Count > 0)
            {
                snappedPosition = PlacementHelpers.SnapPositionToGridSnapPoints(targetGrid, targetPosition);
            }
            else
            {
                // snap position and rotation to grid
                snappedPosition = PlacementHelpers.SnapPositionToGrid(targetGrid, targetPosition, Context.CheckingPlaceable.GridOffset);
            }

            SetPositionAndRotation(snappedPosition, snappedRotation);

            // get issues under pins
            IReadOnlyList<PinPlacementIssue> placementIssues = GetPinIssues(targetGrid, Context.CheckingPlaceable);

            // get list of all overlapping points
            List<GridPoint> overlappingPoints = GetOverlaps<GridPoint>(Context.CheckingPlaceable);

            // check if any of those points are occupied - cannot be placed over closed ports
            bool isValid = true;
            foreach (GridPoint gridPoint in overlappingPoints)
            {
                if (!gridPoint.Open)
                {
                    isValid = false;
                    break;
                }
            }

            bool placementValid = placementIssues.Count == 0 && isValid;

            Context.Placeable.DisplayPlacementStatus(placementValid ? String.Empty : "Placement Invalid", placementValid);

            if (!placementValid)
            {
                foreach (PinPlacementIssue issue in placementIssues)
                {
                    GridPin displayPin = _checkingToRealGridPinMapping[issue.PinWithIssue];
                    displayPin.DisplayIssue(issue.IssueType);
                }
            }
        }

        private IReadOnlyList<GridHost> GetOverlappingGrids(GridPlaceable placeable, Vector3 position)
        {
            // get all grid points that placeable overlaps with.
            List<GridPoint> overlappingGridPoints = GetOverlaps<GridPoint>(position, Context.Size, placeable.transform.rotation.eulerAngles.z);
            HashSet<GridHost> result = new HashSet<GridHost>();

            foreach (GridPoint overlappingGridPoint in overlappingGridPoints)
            {
                if (!overlappingGridPoint.HostingGrid.CablesOnly)
                    result.Add(overlappingGridPoint.HostingGrid);
            }

            return result.ToList();
        }

        /// <summary>
        /// <para>Creates a map of a <see cref="GridPlaceable">GridPlaceables</see> <see cref="GridPin">GridPins</see> to the <see cref="GridPoint"/> found underneath them.</para>
        /// <para>All pins are assumed to be above points.</para>
        /// </summary>
        /// <param name="targetGrid">The grid that all points should be on.</param>
        /// <param name="placeable">The <see cref="GridPlaceable"/> containing the <see cref="GridPin"/> collection to check.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Dictionary<GridPin, GridPoint> GetPointsUnderPins(GridHost targetGrid, GridPlaceable placeable)
        {
            // create results dictionary
            Dictionary<GridPin, GridPoint> result = new Dictionary<GridPin, GridPoint>();

            // loop through all pins
            foreach (GridPin gridPin in placeable.Pins)
            {
                // try and find GridPoint under pin
                GridPoint? point = GetPointAtPosition(gridPin.transform.position, targetGrid);

                // pins are at this point assumed to be valid, throw if not
                if (point == null)
                    throw new InvalidOperationException();

                // add pin and point to result
                result.Add(gridPin, point);
            }

            return result;
        }

        private GridPoint? GetPointAtPosition(Vector2 position, GridHost? filter = null)
        {
            // get all overlaps in radius
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, _pinCheckDistanceRadiusThreshold);

            // loop through all overlaps
            foreach (Collider2D overlap in overlaps)
            {
                // check for grid point
                GridPoint? gridPoint = overlap.GetComponent<GridPoint>();

                // skip if null
                if (gridPoint == null)
                    continue;

                // if filter not set, return
                if (filter == null)
                    return gridPoint;

                // if matches filter, return
                if (gridPoint.HostingGrid == filter)
                    return gridPoint;

                // else skip
            }

            return null;
        }

        private IReadOnlyList<PinPlacementIssue> GetPinIssues(GridHost targetGrid, GridPlaceable placeable)
        {
            // create result list
            List<PinPlacementIssue> result = new List<PinPlacementIssue>();

            // loop through all pins
            foreach (GridPin pin in placeable.Pins)
            {
                // try and get the point under the pin
                GridPoint? point = GetPointAtPosition(pin.transform.position);

                // add to issue list if one is found
                if (point == null)
                    // no point found
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.PORT_NOT_FOUND));
                else if (point.HostingGrid != targetGrid)
                    // point on different grid
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.INVALID_HOST));
                else if (point.Blocked)
                    // point blocked
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.PORT_BLOCKED));
                else if (!point.Open)
                    // point occupied
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.PORT_OCCUPIED));
            }

            return result;
        }

        private GridHost? GetTargetGrid(Vector3 position)
        {
            // get all grids the placeable is currently overlapping.
            IReadOnlyList<GridHost> overlappingGrids = GetOverlappingGrids(Context.CheckingPlaceable, position);

            // if not overlapping grids - can check for floating placement
            if (overlappingGrids.Count == 0)
            {
                return null;
            }

            // stick to last one selected
            if (overlappingGrids.Contains(_previousGridHost))
                return _previousGridHost;

            // otherwise select first
            _previousGridHost = overlappingGrids[0];
            return overlappingGrids[0];
        }
    }
}