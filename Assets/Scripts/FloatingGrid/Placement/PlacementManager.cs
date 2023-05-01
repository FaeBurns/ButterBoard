using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.FloatingGrid.Placement.Services;
using ButterBoard.Lookup;
using ButterBoard.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementManager : SingletonBehaviour<PlacementManager>
    {
        private float _rotationTarget;

        [SerializeField]
        private float pinCheckDistanceRadiusThreshold = 0.1f;

        [SerializeField]
        private float displayZDistance = 1;

        public bool Placing => ActiveService != null;

        public bool CanCancel => Placing && ActiveService!.CanCancel(); // _activeService will only be dereferenced if Placing is true

        public IPlacementService? ActiveService { get; private set; }

        [field: SerializeField]
        public LerpSettings LerpSettings { get; private set; } = null!;

        public void BeginPlace(string assetSourceKey)
        {
            // throw if currently placing
            if (Placing)
                throw new InvalidOperationException("Cannot begin placement while already placing.");

            // fetch prefab to be spawned
            GameObject? prefab = AssetSource.Fetch<GameObject>(assetSourceKey);

            // check if prefab is invalid
            if (prefab == null)
            {
                Debug.LogError($"Failed to find asset with key {{{assetSourceKey}}}");
                return;
            }

            // get placeable
            BasePlaceable prefabPlaceable = prefab.GetComponent<BasePlaceable>();

            // get target service and begin prefab placement
            ActiveService = GetTargetService(prefabPlaceable);
            ActiveService.BeginPrefabPlacement(prefab, assetSourceKey);

            // set initial rotation offset
            _rotationTarget = prefabPlaceable.InitialRotationOffset;

            // deselect any selected UI - stops issues with keyboard not working on rack select
            EventSystem.current.SetSelectedGameObject(null!);

            // disable world canvas interaction
            WorldCanvasHelper.ExecuteOnAll(h => h.DisableInteraction());
        }

        public void BeginMove(GameObject target)
        {
            // throw if currently placing
            if (Placing)
                throw new InvalidOperationException();

            // get placeable
            BasePlaceable placeable = target.GetComponent<BasePlaceable>();

            // get target service and begin move placement
            ActiveService = GetTargetService(placeable);
            ActiveService.BeginMovePlacement(target);

            // set initial rotation
            _rotationTarget = placeable.PlacedRotation;

            // deselect any selected UI - stops issues with keyboard not working on rack select
            EventSystem.current.SetSelectedGameObject(null!);

            // disable world canvas interaction
            WorldCanvasHelper.ExecuteOnAll(h => h.DisableInteraction());
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

            ActiveService?.CancelPlacement();
            ActiveService = null;

            // re-enable world canvas interaction
            WorldCanvasHelper.ExecuteOnAll(h => h.EnableInteraction());
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
                _rotationTarget += ActiveService!.GetPlaceable().RotationStep;
            else if (Input.GetKeyDown(KeyCode.E))
                _rotationTarget -= ActiveService!.GetPlaceable().RotationStep;

            Quaternion rotation = Quaternion.Euler(0, 0, _rotationTarget);

            // run update
            bool finished = ActiveService!.Update(mouseWorldPosition, rotation);

            // if not finished check if it should try and commit placement
            if (!finished)
            {
                // if left mouse is pressed and mouse is not over ui then try and complete placement
                // THIS WILL GET CALLED THE SAME FRAME AS PLACEMENT STARTS IF SELECTED FROM THE RACK
                // TODO: set execution order later. very sleepy rn :3
                // Me from like an hour later - still quite tired - know knows that this is false
                // don't actually need the UI check either
                // The issue was coming from PlacementServices calling MarkRemoval instead of MarkPlacement on PlacementLimitManager
                // too eepy for this rn :pensive:
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    ActiveService.TryCommitPlacement(mouseWorldPosition, rotation);

                return;
            }

            // complete placement
            ActiveService.CompletePlacement();

            // get the newly placed placeable
            BasePlaceable placeable = ActiveService.GetPlaceable();

            // set the final rotation
            placeable.PlacedRotation = _rotationTarget;

            // clear active service
            ActiveService = null;

            // reset rotation
            _rotationTarget = 0;

            WorldCanvasHelper.ExecuteOnAll(h => h.EnableInteraction());
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