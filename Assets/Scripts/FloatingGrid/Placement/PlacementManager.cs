using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.FloatingGrid.Placement.Services;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementManager : SingletonBehaviour<PlacementManager>, IInteractionProvider
    {
        private IPlacementService? _activeService;
        private float _rotationTarget;

        [SerializeField]
        private GameObject? rackUIHost;

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

            _activeService = GetTargetService(prefab.GetComponent<BasePlaceable>());
            _activeService.BeginPrefabPlacement(prefab);
        }

        public void BeginMove(GameObject target)
        {
            // throw if currently placing
            if (Placing)
                throw new InvalidOperationException();

            _activeService = GetTargetService(target.GetComponent<BasePlaceable>());
            _activeService.BeginMovePlacement(target);
        }

        public void Remove(BasePlaceable target)
        {
            // don't need to throw while placing as this should be able to run concurrently
            // a check to see if the one being removed is the one being placed would be nice
            // but that's not possible with the current implementation

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
                _rotationTarget += 90;
            else if (Input.GetKeyDown(KeyCode.E))
                _rotationTarget -= 90;

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

            // complete placement and clear _activeService
            _activeService.CompletePlacement();
            _activeService = null;

            // reset rotation
            _rotationTarget = 0;
        }

        private void SelectUpdate()
        {
            // if mouse button is not pressed
            // return
            if (!Input.GetMouseButtonDown(0))
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

            if (Input.GetKey(KeyCode.R))
                // remove clicked placeable
                Remove(pickupTarget);
            else
                // begin movement
                BeginMove(pickupTarget.gameObject);

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

        public void OnSwitchTo()
        {
            enabled = true;
            if (rackUIHost != null)
            {
                rackUIHost.SetActive(true);
            }
        }

        public void OnSwitchAway()
        {
            if (CanCancel)
                Cancel();

            enabled = false;
            if (rackUIHost != null)
            {
                rackUIHost.SetActive(false);
            }
        }

        public bool CanInteractionSafelySwitch()
        {
            return !Placing || CanCancel;
        }
    }
}