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
        private GameObject _placingObject = null!;
        private GridPlaceable _placingPlaceable = null!;

        private Vector2 _placingSize = Vector2.zero;

        public GridPlaceableDeployer(GameObject placeablePrefab, LerpSettings lerpSettings)
        {
            _placeablePrefab = placeablePrefab;
            _lerpSettings = lerpSettings;
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

        // ReSharper disable Unity.PerformanceAnalysis
        public void UpdatePlacement(Vector3 desiredWorldPosition, Quaternion desiredRotation)
        {
            Vector3 position = Vector3.Lerp(_placingObject.transform.position, desiredWorldPosition + _placingPlaceable.GridOffset, _lerpSettings.TranslateLerp);
            Quaternion rotation = Quaternion.Lerp(_placingObject.transform.rotation, desiredRotation, _lerpSettings.RotateLerp);

            List<GridHost> overlappingGrids = GetOverlappingGrids(desiredWorldPosition, desiredRotation.eulerAngles.z);
            if (overlappingGrids.Count == 0)
            {
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

            List<PinPlacementIssue> pinPlacementIssues = GetInvalidPins(targetGrid);

            Debug.Log($"GetInvalidPins returned with {pinPlacementIssues.Count} issues");

            foreach (PinPlacementIssue placementIssue in pinPlacementIssues)
            {
                Debug.Log(placementIssue.ToString());
            }
        }

        public bool CommitPlacement()
        {
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
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] overlapResults = Physics2D.OverlapBoxAll(location, _placingSize, rotation);

            HashSet<GridHost> hosts = new HashSet<GridHost>();

            foreach (Collider2D collider2D in overlapResults)
            {
                GridPoint? point = collider2D.GetComponent<GridPoint>();
                if (point == null)
                    continue;

                hosts.Add(point.HostGridHost);
            }

            return hosts.ToList();
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

        private GridPoint? GetPointUnderPin(GridHost targetGrid, GridPin pin)
        {
            foreach (GridPoint gridPoint in targetGrid.GridPoints)
            {
                Vector3 offset = pin.transform.position - gridPoint.transform.position;
                float distanceSquared = offset.sqrMagnitude;
                if (distanceSquared <= gridPoint.Radius * gridPoint.Radius)
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
            Vector3 offset = targetGrid.TopLeftOffsetFromCenter;
            Vector3 localPlacementOffset = _placingPlaceable.GridOffset;

            // rotate grid and position back so that they're in default space
            // apply grid snap
            // apply any offsets
            // rotate position back

            // get position relative to grid origin
            Vector3 relativePosition = position - gridPosition;

            // initial position rotation
            Vector3 rotatedPosition = relativePosition.Rotate(-gridRotation);

            // snap position to grid
            Vector3 snappedPosition = SnapPositionToLocalGridOfSize(rotatedPosition, gridScale);

            // offset snapped position
            Vector3 offsetSnappedPosition = snappedPosition + localPlacementOffset;

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