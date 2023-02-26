using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Services;
using UnityEngine;

namespace ButterBoard.Cables
{
    public class CableDisplay : MonoBehaviour
    {
        private Transform _targetA = null!;
        private Transform _targetB = null!;

        [BindComponent]
        private LineRenderer _lineRenderer = null!;

        public void SetTargets(Transform transformA, Transform transformB)
        {
            _targetA = transformA;
            _targetB = transformB;

            // force an update - stops 1 frame graphical errors
            Update();
        }

        private void Awake()
        {
            this.ResolveReferences();
        }

        private void Update()
        {
            _lineRenderer.SetPositions(new Vector3[2]
            {
                _targetA.position,
                _targetB.position,
            });
        }
    }
}