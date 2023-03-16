using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.FloatingGrid.Placement.Services;
using Coil;
using UnityEngine;
using UnityEngine.Events;

namespace ButterBoard.Cables
{
    public class CableDisplay : MonoBehaviour
    {
        private bool _previousWireValue = false;
        private CablePlaceable _startPlaceable = null!;

        private Transform _targetA = null!;
        private Transform _targetB = null!;

        [BindComponent]
        private LineRenderer _lineRenderer = null!;

        [SerializeField]
        private float zOverride = -6;

        public void Initialize(CablePlaceable start, CablePlaceable end)
        {
            start.Remove.AddListener(OnCableRemoved);

            _startPlaceable = start;

            SetColour(_startPlaceable.CableColor);

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

            // peek at value on wire
            BoolValue watchingValue = _startPlaceable.Pin.ConnectedPoint!.Wire.Peek();

            // if the value has changed
            if (watchingValue.Value != _previousWireValue)
            {
                // set the wire colour
                SetColour(watchingValue.Value ? _startPlaceable.PoweredCableColor : _startPlaceable.CableColor);

                _previousWireValue = watchingValue.Value;
            }
        }

        private Vector3 ModifyPosition(Vector3 position)
        {
            return new Vector3(position.x, position.y, zOverride);
        }

        private void OnCableRemoved()
        {
            Destroy(gameObject);
        }

        private void SetColour(Color color)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }
}