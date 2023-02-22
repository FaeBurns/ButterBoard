using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class GridPlacementService : PlacementService<GridPlaceable>
    {
        private readonly float _pinCheckDistanceRadiusThreshold;

        /// <summary>
        /// Gets a mapping of <see cref="GridPin">GridPins</see> on <see cref="PlacementContext{T}.Placeable"/> to <see cref="GridPin">GridPins</see> on <see cref="PlacementContext{T}.DisplayPlaceable"/>.
        /// </summary>
        private readonly Dictionary<GridPin, GridPin> _realToDisplayGridPinMapping = new Dictionary<GridPin, GridPin>();

        public GridPlacementService(LerpSettings lerpSettings, float pinCheckDistanceRadiusThreshold) : base(lerpSettings)
        {
            _pinCheckDistanceRadiusThreshold = pinCheckDistanceRadiusThreshold;
        }

        public override void BeginPrefabPlacement(GameObject prefab)
        {
            base.BeginPrefabPlacement(prefab);

            for (int i = 0; i < Context.Placeable.Pins.Count; i++)
            {
                _realToDisplayGridPinMapping.Add(Context.Placeable.Pins[i], Context.DisplayPlaceable.Pins[i]);
            }
        }

        /// <inheritdoc/>
        public override void BeginMovePlacement(GameObject target)
        {
            base.BeginMovePlacement(target);

            // disconnect all GridPins
            foreach (GridPin gridPin in Context!.Placeable.Pins)
            {
                if (gridPin.ConnectedPoint != null)
                    gridPin.ConnectedPoint.Free();

                gridPin.Free();
            }

            for (int i = 0; i < Context.Placeable.Pins.Count; i++)
            {
                _realToDisplayGridPinMapping.Add(Context.Placeable.Pins[i], Context.DisplayPlaceable.Pins[i]);
            }

            // unblock all points
            foreach (GridPoint point in Context.Placeable.OverlappingPoints)
            {
                point.Blocked = false;
            }
        }

        protected override bool CommitPlacement()
        {
            // get all grids the placeable is currently overlapping.
            IReadOnlyList<GridHost> overlappingGrids = GetOverlappingGrids(Context.Placeable);

            // if not overlapping grids - can check for floating placement
            if (overlappingGrids.Count == 0)
            {
                // if not overlapping, can perform floating placement
                bool isOverlapping = GetOverlaps<Transform>(Context.Placeable).Count > 0;

                // if overlapping, placement is not valid
                if (isOverlapping)
                    return false;

                return true;
            }

            // get first grid
            GridHost gridTarget = overlappingGrids[0];

            bool isValid = GetPinIssues(gridTarget, Context.Placeable).Count == 0;

            if (!isValid)
                return false;

            // get list of all overlapping points
            List<GridPoint> overlappingPoints = GetOverlaps<GridPoint>(Context.Placeable);

            // get mapping of pins to points
            Dictionary<GridPin,GridPoint> pinTargets = GetPointsUnderPins(gridTarget, Context.Placeable);

            // get set of all points targeted by pins
            HashSet<GridPoint> pinPoints = new HashSet<GridPoint>(pinTargets.Values);

            // set points covered by bounds of placeable to blocked
            // but only those not getting connected to pins
            foreach (GridPoint gridPoint in overlappingPoints)
            {
                // skip if found in pinPoints
                if (pinPoints.Contains(gridPoint))
                    continue;

                // mark point as blocked
                gridPoint.Blocked = true;
            }

            // connect all pins to points
            foreach ((GridPin gridPin, GridPoint gridPoint) in pinTargets)
            {
                gridPin.Connect(gridPoint);
                gridPoint.Connect(gridPin);
            }

            Context.DisplayPlaceable.ClearPlacementStatus();
            Context.PlacingObject.transform.SetParent(gridTarget.transform);

            return true;
        }

        protected override void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            // clear pin issues
            // probably don't need to do this every frame
            foreach (GridPin pin in Context.DisplayPlaceable.Pins)
            {
                pin.ClearIssue();
            }

            // get all grids the placeable is currently overlapping.
            IReadOnlyList<GridHost> overlappingGrids = GetOverlappingGrids(Context.Placeable);

            // if there were no overlapping grids found
            if (overlappingGrids.Count == 0)
            {
                // set position and rotation to the raw values and return
                SetPositionAndRotation(targetPosition, targetRotation);

                // check if we're overlapping anything by checking if there are any overlaps with objects with transform components
                // all objects will have transform components
                // if there is at least one, the placeable is overlapping
                bool isOverlapping = GetOverlaps<Transform>(Context.Placeable).Count > 0;

                // set display status
                string placementStatus = isOverlapping ? "Destination occupied" : String.Empty;
                Context.DisplayPlaceable.DisplayPlacementStatus(placementStatus, !isOverlapping);
                return;
            }

            // if any grids were found
            // get the first one
            // TODO: see if there's a better way to decide which grid to use. Closest?
            GridHost targetGrid = overlappingGrids[0];

            // snap position and rotation to grid
            // multiplying quaternions adds them together in the order they are shown
            Vector3 snappedPosition = PlacementHelpers.SnapPositionToGrid(targetGrid, targetPosition, Context.Placeable.GridOffset);
            Quaternion snappedRotation = targetRotation * targetGrid.transform.rotation;

            SetPositionAndRotation(snappedPosition, snappedRotation);

            IReadOnlyList<PinPlacementIssue> placementIssues = GetPinIssues(targetGrid, Context.Placeable);

            bool placementValid = placementIssues.Count == 0;

            Context.DisplayPlaceable.DisplayPlacementStatus(placementValid ? String.Empty : "Placement Invalid", placementValid);

            if (!placementValid)
            {
                foreach (PinPlacementIssue issue in placementIssues)
                {
                    GridPin displayPin = _realToDisplayGridPinMapping[issue.PinWithIssue];
                    displayPin.DisplayIssue(issue.IssueType);
                }
            }
        }

        protected override bool UpdateFinalize()
        {
            // get current transform
            Vector3 currentDisplayPosition = Context.DisplayObject.transform.position;
            Quaternion currentDisplayRotation = Context.DisplayObject.transform.rotation;

            // get target transform
            Vector3 targetPosition = Context.PlacingObject.transform.position;
            Quaternion targetRotation = Context.PlacingObject.transform.rotation;

            // get lerp target
            Vector3 lerpPosition = Vector3.Lerp(currentDisplayPosition, targetPosition, LerpSettings.TranslateLerp);
            Quaternion lerpRotation = Quaternion.Lerp(currentDisplayRotation, targetRotation, LerpSettings.RotateLerp);

            // set display object to use lerp data
            Context.DisplayObject.transform.position = lerpPosition;
            Context.DisplayObject.transform.rotation = lerpRotation;

            // check approximate position and rotation
            bool approximatePosition = currentDisplayPosition.ApproximateDistance(targetPosition, 0.01f);
            bool approximateRotation = Quaternion.Dot(currentDisplayRotation, targetRotation).Approximately(1);

            if (approximatePosition && approximateRotation)
            {
                Debug.Log($"current: {currentDisplayPosition} | lerp: {lerpPosition} | target {targetPosition}");

                // don't need to bother updating position/rotation
                // display object is deleted immediately after
                return true;
            }

            return false;
        }

        private IReadOnlyList<GridHost> GetOverlappingGrids(GridPlaceable placeable)
        {
            // get all grid points that placeable overlaps with.
            List<GridPoint> overlappingGridPoints = GetOverlaps<GridPoint>(placeable);
            HashSet<GridHost> result = new HashSet<GridHost>();

            foreach (GridPoint overlappingGridPoint in overlappingGridPoints)
            {
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
    }
}