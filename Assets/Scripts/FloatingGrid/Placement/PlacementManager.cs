using System;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.FloatingGrid.Placement.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementManager : SingletonBehaviour<PlacementManager>, IInteractionProvider
    {
        private IPlacementService? _activeService;
        private float _rotationTarget;

        [FormerlySerializedAs("uiHost")]
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

            _activeService = GetTargetService(prefab);
            _activeService.BeginPrefabPlacement(prefab);
        }

        public void BeginMove(GameObject target)
        {
            // throw if currently placing
            if (Placing)
                throw new InvalidOperationException();

            _activeService = GetTargetService(target);
            _activeService.BeginMovePlacement(target);
        }

        /// <summary>
        /// Cancels placement and deletes the held object
        /// </summary>
        public void CancelPlace()
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
                PickupUpdate();
                return;
            }

            // cancel placement if space is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Cancelling");
                if (CanCancel)
                    CancelPlace();
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

        private void PickupUpdate()
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
            List<BasePlaceable> placeables = GetOverlaps<BasePlaceable>(mouseWorldPosition, Vector2.zero, 0f);

            // exit early to avoid having to sort
            if (placeables.Count == 0)
                return;

            // order collection of found placeables by priority
            IOrderedEnumerable<BasePlaceable> orderedPlaceables = placeables.OrderByDescending(GetPriority);

            // get first target
            BasePlaceable pickupTarget = orderedPlaceables.First();

            // begin movement
            BeginMove(pickupTarget.gameObject);
        }

        private IPlacementService GetTargetService(GameObject target)
        {
            BasePlaceable basePlaceable = target.GetComponent<BasePlaceable>();

            switch (basePlaceable)
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

        private int GetPriority(BasePlaceable placeable)
        {
            return placeable switch
            {
                FloatingPlaceable => 0,
                GridPlaceable => 1,
                CablePlaceable => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(placeable), placeable, null),
            };
        }

        private List<TComponent> GetOverlaps<TComponent>(Vector2 position, Vector2 size, float rotation)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(position, size, rotation);
            List<TComponent> result = new List<TComponent>();
            foreach (Collider2D overlap in overlaps)
            {
                TComponent component = overlap.GetComponent<TComponent>();
                if (component != null)
                    result.Add(component);
            }
            return result;
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
                CancelPlace();

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