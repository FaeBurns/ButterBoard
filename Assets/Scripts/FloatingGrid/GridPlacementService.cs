﻿using System;
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

        [SerializeField]
        private float checkDistanceRadiusThreshold = 0.01f;

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

            _activeDeployer = new GridPlaceableDeployer(placementPrefab, lerpSettings, checkDistanceRadiusThreshold);
            _activeDeployer.BeginPlace();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Update()
        {
            // exit if not placing
            if (!Placing)
            {
                GridPlaceable? placeable = CheckForMove();

                // exit if nothing found
                if (placeable == null)
                    return;

                _activeDeployer = new GridPlaceableDeployer(null!, lerpSettings, checkDistanceRadiusThreshold);
                _activeDeployer.BeginMove(placeable);

                return;
            }

            Vector3 mouseWorldPosition = GetMouseWorldPosition();

            // modify rotation target when input occurs
            if (Input.GetKeyDown(KeyCode.Q))
                _rotationTarget += 90;
            else if (Input.GetKeyDown(KeyCode.E))
                _rotationTarget -= 90;

            _activeDeployer!.UpdatePlacement(mouseWorldPosition, Quaternion.Euler(0, 0, _rotationTarget));

            if (Input.GetMouseButtonDown(0))
            {
                bool success = _activeDeployer!.CommitPlacement();
                if (success)
                {
                    _activeDeployer.EndPlace();
                    _activeDeployer = null;
                }
            }
        }

        private GridPlaceable? CheckForMove()
        {
            // return if no click input
            if (!Input.GetMouseButtonDown(0))
                return null;

            Vector2 mousePosition = GetMouseWorldPosition();
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(mousePosition, 0.1f);

            foreach (Collider2D overlap in overlaps)
            {
                GridPlaceable? placeable = overlap.GetComponent<GridPlaceable>();
                if (placeable != null)
                {
                    return placeable;
                }
            }

            return null;
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