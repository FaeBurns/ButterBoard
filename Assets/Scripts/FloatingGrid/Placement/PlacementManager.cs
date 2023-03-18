using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.FloatingGrid.Placement.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementManager : SingletonBehaviour<PlacementManager>
    {
        private IPlacementService? _activeService;
        private float _rotationTarget;

        [SerializeField]
        private float pinCheckDistanceRadiusThreshold = 0.1f;

        [SerializeField]
        private float displayZDistance = 1;

        public bool Placing => _activeService != null;

        public bool CanCancel => Placing && _activeService!.CanCancel(); // _activeService will only be dereferenced if Placing is true

        [field: SerializeField]
        public LerpSettings LerpSettings { get; private set; } = null!;

        public void BeginPlace(GameObject prefab)
        {
            // throw if currently placing
            if (Placing)
                throw new InvalidOperationException();

            // get placeable
            BasePlaceable prefabPlaceable = prefab.GetComponent<BasePlaceable>();

            // get target service and begin prefab placement
            _activeService = GetTargetService(prefabPlaceable);
            _activeService.BeginPrefabPlacement(prefab);

            // set initial rotation offset
            _rotationTarget = prefabPlaceable.InitialRotationOffset;
        }

        public void BeginMove(GameObject target)
        {
            // throw if currently placing
            if (Placing)
                throw new InvalidOperationException();

            // get placeable
            BasePlaceable placeable = target.GetComponent<BasePlaceable>();

            // get target service and begin move placement
            _activeService = GetTargetService(placeable);
            _activeService.BeginMovePlacement(target);

            // set initial rotation
            _rotationTarget = placeable.PlacedRotation;
        }

        public void Remove(BasePlaceable target)
        {
            // don't need to throw while placing as this should be able to run concurrently
            // a check to see if the one being removed is the one being placed would be nice
            // but that's not possible with the current implementation
            // kinda stinky that it has to create a service for this
            // should probably be a static method but those can't be abstract/overriden
            // or use something else - RemovalService?
            // don't like that tho, would split things uncomfortably

            IPlacementService placementService = GetTargetService(target);
            placementService.Remove(target);
        }

        /// <summary>
        /// Cancels placement and deletes the held object
        /// </summary>
        public void Cancel()
        {
            // throw if cancelling is not allowed
            if (!CanCancel)
                throw new InvalidOperationException();

            _activeService?.CancelPlacement();
            _activeService = null;
        }

        private void Update()
        {
            // return if not placing
            if (!Placing)
            {
                SelectUpdate();
                return;
            }

            PlacingUpdate();
        }

        private void PlacingUpdate()
        {
            // cancel placement if space is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Cancelling");
                if (CanCancel)
                    Cancel();
                return;
            }

            // get mouse pos in world
            Vector3 mouseWorldPosition = PlacementHelpers.GetMouseWorldPosition();

            // modify rotation target when input occurs
            if (Input.GetKeyDown(KeyCode.Q))
                _rotationTarget += _activeService!.GetPlaceable().RotationStep;
            else if (Input.GetKeyDown(KeyCode.E))
                _rotationTarget -= _activeService!.GetPlaceable().RotationStep;

            Quaternion rotation = Quaternion.Euler(0, 0, _rotationTarget);

            // run update
            bool finished = _activeService!.Update(mouseWorldPosition, rotation);

            // if not finished check if it should try and commit placement
            if (!finished)
            {
                if (Input.GetMouseButtonDown(0))
                    _activeService.TryCommitPlacement(mouseWorldPosition, rotation);

                return;
            }

            // complete placement
            _activeService.CompletePlacement();

            // set the final rotation
            _activeService.GetPlaceable().PlacedRotation = _rotationTarget;

            // clear active service
            _activeService = null;

            // reset rotation
            _rotationTarget = 0;
        }

        private void SelectUpdate()
        {
            bool leftMouse = Input.GetMouseButtonDown(0);
            bool rightMouse = Input.GetMouseButtonDown(1);

            // if no mouse button is pressed then exit early
            if (!(leftMouse || rightMouse))
                return;


            // if mouse is over ui, exit
            if (EventSystem.current != null)
            {
                bool pointOverUI = EventSystem.current.IsPointerOverGameObject();
                if (pointOverUI)
                    return;
            }

            Vector3 mouseWorldPosition = PlacementHelpers.GetMouseWorldPosition();

            // get all placeables under cursor
            // size of zero still allows for collision checks
            List<BasePlaceable> placeables = PlacementHelpers.GetOverlaps<BasePlaceable>(mouseWorldPosition, Vector2.zero, 0f);

            // exit early to avoid having to sort
            if (placeables.Count == 0)
                return;

            // get sorted by highest priority
            BasePlaceable pickupTarget = PlacementHelpers.GetHighestPriority(placeables);

            // left mouse binds to pickup/movement
            if (leftMouse)
                // begin movement
                BeginMove(pickupTarget.gameObject);

            // right mouse binds to removal
            else if (rightMouse)
                Remove(pickupTarget);
        }

        private IPlacementService GetTargetService(BasePlaceable target)
        {
            switch (target)
            {
                case GridPlaceable:
                    return new GridPlacementService(LerpSettings, pinCheckDistanceRadiusThreshold, displayZDistance);
                case FloatingPlaceable:
                    return new FloatingPlacementService(LerpSettings, displayZDistance);
                case CablePlaceable:
                    return new CablePlacementService(LerpSettings, pinCheckDistanceRadiusThreshold, displayZDistance);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}