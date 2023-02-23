using System;
using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using ButterBoard.Cables;
using ButterBoard.FloatingGrid.Placement.Services;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementManager : ReferenceResolvedBehaviour
    {
        private IPlacementService? _activeService;
        private float _rotationTarget;

        [SerializeField]
        private float pinCheckDistanceRadiusThreshold = 0.1f;

        [SerializeField]
        private float displayZDistance = 1;

        public bool Placing => _activeService != null;

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
            _activeService.BeginPrefabPlacement(target);
        }

        /// <summary>
        /// Cancels placement and deletes the held object
        /// </summary>
        public void CancelPlace()
        {
            // throw if currently placing
            if (!Placing)
                throw new InvalidOperationException();

            _activeService?.CancelPlacement();
            _activeService = null;
        }

        public BasePlaceable GetTargetUnderCursor()
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            // return if not placing
            if (!Placing)
                return;

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
                    _activeService.TryCommitPlacement();

                return;
            }

            // complete placement and clear _activeService
            _activeService.CompletePlacement();
            _activeService = null;
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
                    return new CablePlacementService(LerpSettings, displayZDistance);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}