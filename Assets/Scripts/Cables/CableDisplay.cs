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
        private CablePlaceable _startPlaceable = null!;

        private Transform _targetA = null!;
        private Transform _targetB = null!;

        [BindComponent]
        private LineRenderer _lineRenderer = null!;

        [SerializeField]
        private float zOverride = -6;

        public void Initialize(CablePlaceable start, CablePlaceable end)
        {
            _startPlaceable = start;
            _lineRenderer.startColor = _startPlaceable.CableColor;
            _lineRenderer.endColor = _startPlaceable.CableColor;

            _targetA = start.transform;
            _targetB = end.transform;

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
                ModifyPosition(_targetA.position),
                ModifyPosition(_targetB.position),
            });
        }

        private Vector3 ModifyPosition(Vector3 position)
        {
            return new Vector3(position.x, position.y, zOverride);
        }
    }
}