using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeanCore.Unity.ReferenceResolver;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementService : MonoBehaviour
    {
        private Camera _camera = null!;

        private GridPlacementContext? _context = null;
        private float _rotationTarget = 0;

        [SerializeField]
        private LerpSettings lerpSettings = new LerpSettings();

        [SerializeField]
        private float checkDistanceRadiusThreshold = 0.01f;

        public bool Placing => _context != null;

        private void Awake()
        {
            _camera = Camera.main!;
            ReferenceStore.RegisterReference(this);
        }

        public void BeginPrefabPlacement(GameObject placementPrefab)
        {
            if (Placing)
                throw new InvalidOperationException($"Cannot begin placement while already placing. Use the property {nameof(Placing)} to check if it is okay to begin.");

            // instantiate object
            GameObject placingObject = Instantiate(placementPrefab);

            _context = new GridPlacementContext(placingObject, placingObject.GetComponent<GridPlaceable>());
        }

        public void BeginMovePlacement(GridPlaceable target)
        {
            if (Placing)
                throw new InvalidOperationException($"Cannot begin move operation while already placing.");

            GameObject placingObject = target.gameObject;
            _context = new GridPlacementContext(placingObject, target);

            // disconnect all pins
            foreach (GridPin pin in target.Pins)
            {
                GridPoint? point = pin.ConnectedPoint;
                if (point != null)
                {
                    point.Free();
                }
                pin.Free();
            }

            // unblock all points
            foreach (GridPoint point in target.OverlappingPoints)
            {
                point.Blocked = false;
            }

            // clear parent of placeable being moved
            placingObject.transform.SetParent(null);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Update()
        {
            // exit if not placing
            if (!Placing)
            {
                TryPickUpUnderCursor();
                return;
            }

            // if not valid state then exit
            if (_context!.State != GridPlacementState.POSITION)
                return;

            Vector3 mouseWorldPosition = GetMouseWorldPosition();

            // modify rotation target when input occurs
            if (Input.GetKeyDown(KeyCode.Q))
                _rotationTarget += 90;
            else if (Input.GetKeyDown(KeyCode.E))
                _rotationTarget -= 90;

            Quaternion rotation = Quaternion.Euler(0, 0, _rotationTarget);

            UpdatePlacement(mouseWorldPosition, rotation);

            if (Input.GetMouseButtonDown(0))
            {
                GridPlaceable? placementResult = CommitPlacement(mouseWorldPosition, rotation);
                if (placementResult != null)
                {
                    Vector3 finalPosition = SnapHelpers.SnapPositionToGrid(placementResult.PlacedGrid!, mouseWorldPosition, placementResult.GridOffset);
                    Quaternion finalRotation = rotation.Add(placementResult.PlacedGrid!.transform.rotation);
                    _context.State = GridPlacementState.FINALIZE;
                    _context.Placeable.SetPlaceStatus(Color.white);
                    StartCoroutine(FinalPlacementLerp(placementResult.gameObject, finalPosition, finalRotation));
                }
            }
        }

        private void UpdatePlacement(Vector3 desiredWorldPosition, Quaternion desiredRotation)
        {
            Vector3 position = Vector3.Lerp(_context!.PlacingObject.transform.position, desiredWorldPosition + _context.Placeable.GridOffset, lerpSettings.TranslateLerp);
            Quaternion rotation = Quaternion.Lerp(_context.PlacingObject.transform.rotation, desiredRotation, lerpSettings.RotateLerp);

            List<GridHost> overlappingGrids = GetOverlappingGrids(desiredWorldPosition, desiredRotation.eulerAngles.z);
            if (overlappingGrids.Count == 0)
            {
                _context.Placeable.SetPlaceStatus(Color.white);
                _context.PlacingObject.transform.position = position;
                _context.PlacingObject.transform.rotation = rotation;
                return;
            }

            // have to pick one so pick the first one found
            // could also just not snap in that case
            // will need to test
            GridHost targetGrid = overlappingGrids[0];

            Vector3 snappedDesiredPosition = SnapHelpers.SnapPositionToGrid(targetGrid, desiredWorldPosition, _context.Placeable.GridOffset);
            position = Vector3.Lerp(_context.PlacingObject.transform.position, snappedDesiredPosition, lerpSettings.TranslateLerp);
            rotation = Quaternion.Lerp(_context.PlacingObject.transform.rotation, Quaternion.Euler(desiredRotation.eulerAngles + targetGrid.transform.rotation.eulerAngles), lerpSettings.RotateLerp);

            _context.PlacingObject.transform.position = position;
            _context.PlacingObject.transform.rotation = rotation;

            // set tint colour to green or red depending on if placement is valid
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (CheckValidPlacement(targetGrid))
            {
                _context.Placeable.SetPlaceStatus(Color.green);
            }
            else
            {
                _context.Placeable.SetPlaceStatus(Color.red);
            }

            List<PinPlacementIssue> issues = GetInvalidPins(targetGrid);
            Debug.Log(issues.Count);
            foreach (PinPlacementIssue issue in issues)
            {
                Debug.Log(issue.ToString());
            }
        }

        /// <summary>
        /// Checks to see if placement is valid and apply if so.
        /// </summary>
        /// <param name="desiredPosition">The desired position to place into.</param>
        /// <param name="desiredRotation">The desired rotation.</param>
        /// <returns>The placed <see cref="GridPlaceable"/>.</returns>
        public GridPlaceable? CommitPlacement(Vector3 desiredPosition, Quaternion desiredRotation)
        {
            // get all grids under placeable
            List<GridHost> overlappingGrids = GetOverlappingGrids(desiredPosition, desiredRotation.eulerAngles.z);

            // exit if none found
            if (overlappingGrids.Count == 0)
                return null;

            // get first if any were found
            GridHost targetGrid = overlappingGrids[0];

            // record current position and rotation
            Vector3 oldPosition = _context!.PlacingObject.transform.position;
            Quaternion oldRotation = _context.PlacingObject.transform.rotation;

            // snap position and rotation to desired
            _context.PlacingObject.transform.position = SnapHelpers.SnapPositionToGrid(targetGrid, desiredPosition, _context.Placeable.GridOffset);
            _context.PlacingObject.transform.rotation = desiredRotation.Add(targetGrid.transform.rotation);

            // check if placement is valid
            if (CheckValidPlacement(targetGrid))
            {
                // get list of all points overlapped by bounds
                List<GridPoint> overlappingPoints = GetOverlappingPoints(desiredPosition, desiredRotation.eulerAngles.z);

                // get map of all pins and their target points
                Dictionary<GridPin, GridPoint> pinTargets = GetPointsUnderPins(targetGrid);

                // get set of all points targeted by pins
                HashSet<GridPoint> pinPoints = new HashSet<GridPoint>(pinTargets.Values);

                // set points covered by bounds of placeable to blocked
                // but only those not getting connected to pins
                foreach (GridPoint gridPoint in overlappingPoints)
                {
                    // skip if found in pinPoints
                    if (pinPoints.Contains(gridPoint))
                        continue;

                    gridPoint.Blocked = true;
                }

                // connect all pins to points
                foreach ((GridPin gridPin, GridPoint gridPoint) in pinTargets)
                {
                    gridPin.Connect(gridPoint);
                    gridPoint.Connect(gridPin);
                }

                _context.PlacingObject.transform.SetParent(targetGrid.transform);

                _context.Placeable.OverlappingPoints = overlappingPoints.ToArray();

                // restore position and rotation to allow for position lerp to be handled by coroutine
                _context.PlacingObject.transform.position = oldPosition;
                _context.PlacingObject.transform.rotation = oldRotation;

                // record target grid
                _context.Placeable.PlacedGrid = targetGrid;

                return _context.Placeable;
            }

            // restore position and rotation as no placement was performed
            _context.PlacingObject.transform.position = oldPosition;
            _context.PlacingObject.transform.rotation = oldRotation;

            return null;
        }

        private void TryPickUpUnderCursor()
        {
            // return if no click input
            if (!Input.GetMouseButtonDown(0))
                return;

            Vector2 mousePosition = GetMouseWorldPosition();
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(mousePosition, 0.1f);

            foreach (Collider2D overlap in overlaps)
            {
                GridPlaceable? placeable = overlap.GetComponent<GridPlaceable>();
                if (placeable != null)
                {
                    BeginMovePlacement(placeable);
                    return;
                }
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 screenPosition = Input.mousePosition;
            Vector3 result = _camera.ScreenToWorldPoint(screenPosition);
            result.z = 0;
            return result;
        }

        private IEnumerator FinalPlacementLerp(GameObject targetObject, Vector3 targetPosition, Quaternion targetRotation)
        {
            float lerpScalar = 0f;
            while (true)
            {
                Transform targetTransform = targetObject.transform;

                float translateAlpha = Mathf.Lerp(lerpSettings.TranslateLerp, 1, lerpScalar);
                float rotateAlpha = Mathf.Lerp(lerpSettings.RotateLerp, 1, lerpScalar);

                targetTransform.position = Vector3.Lerp(targetObject.transform.position, targetPosition, translateAlpha);
                targetTransform.rotation = Quaternion.Lerp(targetObject.transform.rotation, targetRotation, rotateAlpha);

                // check if stop is possible
                bool approximatePosition = targetTransform.position.Approximately(targetPosition, 0.1f);
                bool approximateRotation = targetTransform.rotation.Approximately(targetRotation);
                if (approximatePosition && approximateRotation)
                {
                    targetObject.transform.position = targetPosition;
                    targetObject.transform.rotation = targetRotation;

                    _context = null;

                    yield break;
                }

                // increase lerpScalar by time
                lerpScalar += Time.deltaTime * lerpSettings.FinalizeScalarTime;

                // skip frame
                yield return null;
            }
        }

        private bool CheckValidPlacement(GridHost gridHost)
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
            Collider2D[] overlapResults = Physics2D.OverlapBoxAll(location, _context!.Size, rotation);

            List<GridPoint> result = new List<GridPoint>();

            foreach (Collider2D overlap in overlapResults)
            {
                GridPoint? point = overlap.GetComponent<GridPoint>();
                if (point != null)
                    result.Add(point);
            }

            return result;
        }

        private List<PinPlacementIssue> GetInvalidPins(GridHost targetGrid)
        {
            List<PinPlacementIssue> result = new List<PinPlacementIssue>();
            foreach (GridPin pin in _context!.Placeable.Pins)
            {
                // try and get the point under the pin
                GridPoint? point = GetPointAtPosition(targetGrid, pin.transform.position);

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
            foreach (GridPin gridPin in _context!.Placeable.Pins)
            {
                GridPoint? point = GetPointAtPosition(targetGrid, gridPin.transform.position);

                if (point == null)
                    throw new InvalidOperationException();

                result.Add(gridPin, point);
            }

            return result;
        }

        private GridPoint? GetPointAtPosition(GridHost targetGrid, Vector3 position)
        {
            foreach (GridPoint gridPoint in targetGrid.GridPoints)
            {
                Vector3 offset = position - gridPoint.transform.position;
                float distanceSquared = offset.sqrMagnitude;
                if (distanceSquared <= (gridPoint.Radius * gridPoint.Radius) * checkDistanceRadiusThreshold)
                {
                    return gridPoint;
                }
            }

            return null;
        }
    }

}