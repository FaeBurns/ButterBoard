using System;
using System.Diagnostics.CodeAnalysis;
using BeanCore.Unity.ReferenceResolver;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPlacementService : MonoBehaviour
    {
        private Camera _camera = null!;

        private GridPlaceableDeployer? _activeDeployer;
        private float _rotationTarget = 0;

        [SerializeField]
        private LerpSettings lerpSettings = new LerpSettings();

        public bool Placing => _activeDeployer != null;

        private void Awake()
        {
            _camera = Camera.main!;
            ReferenceStore.RegisterReference(this);
        }

        public void BeginPlacement(GameObject placementPrefab)
        {
            if (Placing)
                throw new InvalidOperationException($"Cannot begin placement while already placing. Use the property {nameof(Placing)} to check if it is okay to begin.");

            _activeDeployer = new GridPlaceableDeployer(placementPrefab, lerpSettings);
            _activeDeployer.BeginPlace();
        }

        private void Update()
        {
            // exit if not placing
            if (!Placing)
                return;

            Vector3 mouseWorldPosition = GetMouseWorldPosition();

            // modify rotation target when input occurs
            if (Input.GetKeyDown(KeyCode.Q))
                _rotationTarget += 90;
            else if (Input.GetKeyDown(KeyCode.E))
                _rotationTarget -= 90;

            _activeDeployer!.UpdatePlacement(mouseWorldPosition, Quaternion.Euler(0, 0, _rotationTarget));
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 screenPosition = Input.mousePosition;
            Vector3 result = _camera.ScreenToWorldPoint(screenPosition);
            result.z = 0;
            return result;
        }
    }
}