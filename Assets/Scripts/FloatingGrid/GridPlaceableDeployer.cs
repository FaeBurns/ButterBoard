using System;
using System.Collections.Generic;
using System.Linq;
using BeanCore.Unity.ReferenceResolver;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPlaceableDeployer
    {
        private readonly GameObject _placeablePrefab;
        private readonly LerpSettings _lerpSettings;
        private readonly float _checkDistanceRadiusThreshold;
        private GameObject _placingObject = null!;
        private GridPlaceable _placingPlaceable = null!;

        private Vector2 _placingSize = Vector2.zero;

        public GridPlaceableDeployer(GameObject placeablePrefab, LerpSettings lerpSettings, float checkDistanceRadiusThreshold)
        {
            _placeablePrefab = placeablePrefab;
            _lerpSettings = lerpSettings;
            _checkDistanceRadiusThreshold = checkDistanceRadiusThreshold;
        }

        public bool IsPlacing { get; private set; } = false;

        public void BeginPlace()
        {
            if (IsPlacing)
                return;

            // ReSharper disable once AccessToStaticMemberViaDerivedType
            _placingObject = GameObject.Instantiate(_placeablePrefab);
            _placingPlaceable = _placingObject.GetComponent<GridPlaceable>();

            _placingSize = _placingPlaceable.BoundsCollider.bounds.size;

            IsPlacing = true;
        }

        public void BeginMove(GridPlaceable movingPlaceable)
        {
            if (IsPlacing)
                return;

            _placingObject = movingPlaceable.gameObject;
            _placingPlaceable = movingPlaceable;
            _placingSize = _placingPlaceable.BoundsCollider.bounds.size;

            foreach (GridPin pin in movingPlaceable.Pins)
            {
                GridPoint? point = pin.ConnectedPoint;
                if (point != null)
                {
                    point.Free();
                }
                pin.Free();
            }

            foreach (GridPoint point in GetOverlappingPoints(movingPlaceable.transform.position, movingPlaceable.transform.rotation.eulerAngles.z))
            {
                point.Blocked = false;
            }

            IsPlacing = true;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void UpdatePlacement(Vector3 desiredWorldPosition, Quaternion desiredRotation)
        {
            Vector3 position = Vector3.Lerp(_placingObject.transform.position, desiredWorldPosition + _placingPlaceable.GridOffset, _lerpSettings.TranslateLerp);
            Quaternion rotation = Quaternion.Lerp(_placingObject.transform.rotation, desiredRotation, _lerpSettings.RotateLerp);

            List<GridHost> overlappingGrids = GetOverlappingGrids(desiredWorldPosition, desiredRotation.eulerAngles.z);
            if (overlappingGrids.Count == 0)
            {
                _placingPlaceable.SetSpriteColor(Color.white);
                _placingObject.transform.position = position;
                _placingObject.transform.rotation = rotation;
                return;
            }

            // have to pick one so pick the first one found
            // could also just not snap in that case
            // will need to test
            GridHost targetGrid = overlappingGrids[0];

            Vector3 snappedDesiredPosition = SnapPositionToGrid(targetGrid, desiredWorldPosition);
            position = Vector3.Lerp(_placingObject.transform.position, snappedDesiredPosition, _lerpSettings.TranslateLerp);
            rotation = Quaternion.Lerp(_placingObject.transform.rotation, Quaternion.Euler(desiredRotation.eulerAngles + targetGrid.transform.rotation.eulerAngles), _lerpSettings.RotateLerp);

            _placingObject.transform.position = position;
            _placingObject.transform.rotation = rotation;

            if (CheckValidPlacement(targetGrid))
            {
                _placingPlaceable.SetSpriteColor(Color.green);
            }
            else
            {
                _placingPlaceable.SetSpriteColor(Color.red);
            }

            List<PinPlacementIssue> issues = GetInvalidPins(targetGrid);
            Debug.Log(issues.Count);
            foreach (PinPlacementIssue issue in issues)
            {
                Debug.Log(issue.ToString());
            }
        }

        public bool CommitPlacement()
        {
            // get all grids under placeable
            List<GridHost> overlappingGrids = GetOverlappingGrids(_placingObject.transform.position, _placingObject.transform.rotation.eulerAngles.z);

            // exit if none found
            if (overlappingGrids.Count == 0)
                return false;

            // get first if any were found
            GridHost targetGrid = overlappingGrids[0];

            // check if placement is valid
            if (CheckValidPlacement(targetGrid))
            {
                List<GridPoint> overlappingPoints = GetOverlappingPoints(_placingObject.transform.position, _placingObject.transform.rotation.eulerAngles.z);
                Dictionary<GridPin, GridPoint> pinTargets = GetPointsUnderPins(targetGrid);
                HashSet<GridPoint> pinPoints = new HashSet<GridPoint>(pinTargets.Values);

                foreach (GridPoint gridPoint in overlappingPoints)
                {
                    // skip if found in pinPoints
                    if (pinPoints.Contains(gridPoint))
                        continue;

                    gridPoint.Blocked = true;
                }

                foreach ((GridPin gridPin, GridPoint gridPoint) in pinTargets)
                {
                    gridPin.Connect(gridPoint);
                    gridPoint.Connect(gridPin);
                }

                _placingObject.transform.SetParent(targetGrid.transform);

                return true;
            }

            return false;
        }

        public void EndPlace()
        {
            if (!IsPlacing)
                return;

            IsPlacing = false;
        }

        public bool CheckValidPlacement(GridHost gridHost)
        {
            return GetInvalidPins(gridHost).Count == 0;
        }

        private List<GridHost> GetOverlappingGrids(Vector2 location, float rotation)
        {
            HashSet<GridHost> hosts = new HashSet<GridHost>();

            foreach (GridPoint point in GetOverlappingPoints(location, rotation))
            {
                hosts.Add(point.HostGridHost);
            }

            return hosts.ToList();
        }

        private List<GridPoint> GetOverlappingPoints(Vector2 location, float rotation)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] overlapResults = Physics2D.OverlapBoxAll(location, _placingSize, rotation);

            List<GridPoint> result = new List<GridPoint>();

            foreach (Collider2D collider2D in overlapResults)
            {
                GridPoint? point = collider2D.GetComponent<GridPoint>();
                if (point != null)
                    result.Add(point);
            }

            return result;
        }

        public List<PinPlacementIssue> GetInvalidPins(GridHost targetGrid)
        {
            List<PinPlacementIssue> result = new List<PinPlacementIssue>();
            foreach (GridPin pin in _placingPlaceable.Pins)
            {
                // try and get the point under the pin
                GridPoint? point = GetPointUnderPin(targetGrid, pin);

                // add to issue list if one is found
                if (point == null)
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.PORT_NOT_FOUND));
                else if (point.Blocked)
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.PORT_BLOCKED));
                else if (!point.Open)
                    result.Add(new PinPlacementIssue(pin, point, PinPlacementIssueType.PORT_OCCUPIED));
            }

            return result;
        }

        private Dictionary<GridPin, GridPoint> GetPointsUnderPins(GridHost targetGrid)
        {
            Dictionary<GridPin, GridPoint> result = new Dictionary<GridPin, GridPoint>();
            foreach (GridPin gridPin in _placingPlaceable.Pins)
            {
                GridPoint? point = GetPointUnderPin(targetGrid, gridPin);

                if (point == null)
                    throw new InvalidOperationException();

                result.Add(gridPin, point);
            }

            return result;
        }

        private GridPoint? GetPointUnderPin(GridHost targetGrid, GridPin pin)
        {
            foreach (GridPoint gridPoint in targetGrid.GridPoints)
            {
                Vector3 offset = pin.transform.position - gridPoint.transform.position;
                float distanceSquared = offset.sqrMagnitude;
                if (distanceSquared <= (gridPoint.Radius * gridPoint.Radius) * _checkDistanceRadiusThreshold)
                {
                    return gridPoint;
                }
            }

            return null;
        }

        private Vector3 SnapPositionToGrid(GridHost targetGrid, Vector3 position)
        {
            Transform gridTransform = targetGrid.transform;
            Vector3 gridPosition = gridTransform.position;
            float gridRotation = gridTransform.rotation.eulerAngles.z;
            float gridScale = targetGrid.Spacing;
            Vector3 offset = targetGrid.TopLeftOffsetFromCenter.Mod(gridScale);
            Vector3 localPlacementOffset = _placingPlaceable.GridOffset;

            // rotate grid and position back so that they're in default space
            // apply grid snap
            // apply any offsets
            // rotate position back

            // get position relative to grid origin
            Vector3 relativePosition = (position + offset) - gridPosition;

            // initial position rotation
            Vector3 rotatedPosition = relativePosition.Rotate(-gridRotation);

            // snap position to grid
            Vector3 snappedPosition = SnapPositionToLocalGridOfSize(rotatedPosition, gridScale);

            // offset snapped position
            Vector3 offsetSnappedPosition = (snappedPosition + localPlacementOffset) - offset;

            // rotate position back to where it was before (now with snap applied)
            Vector3 gridRotatedPosition = offsetSnappedPosition.Rotate(gridRotation);

            // re-add offset remove in step one
            Vector3 gridRelativePosition = gridRotatedPosition + gridPosition;

            // return result
            return gridRelativePosition;
        }

        private Vector3 SnapPositionToLocalGridOfSize(Vector3 position, float size)
        {
            return new Vector3(Mathf.Round(position.x) * size, Mathf.Round(position.y) * size, Mathf.Round(position.z) * size);
        }

        private float ClampRotation(float rotation)
        {
            int scale = Mathf.FloorToInt(rotation / 360f);
            rotation = rotation - (scale * 360);

            if (rotation > 360)
                return rotation - 360;
            if (rotation < 0)
                return rotation + 360;

            return rotation;
        }
    }

    public class PinPlacementIssue
    {
        public GridPin PinWithIssue { get; }
        public GridPoint? PointProvidingIssue { get; }
        public PinPlacementIssueType IssueType { get; }

        public PinPlacementIssue(GridPin pinWithIssue, GridPoint? pointProvidingIssue, PinPlacementIssueType issueType)
        {
            PinWithIssue = pinWithIssue;
            PointProvidingIssue = pointProvidingIssue;
            IssueType = issueType;
        }

        public override string ToString()
        {
            return IssueType.ToString();
        }
    }

    public enum PinPlacementIssueType
    {
        EXISTING_INVALID_CONNECTION,
        PORT_NOT_FOUND,
        PORT_OCCUPIED,
        PORT_BLOCKED,
    }
}