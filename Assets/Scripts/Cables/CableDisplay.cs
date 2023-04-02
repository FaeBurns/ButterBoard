using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Simulation;
using UnityEngine;

namespace ButterBoard.Cables
{
    public class CableDisplay : MonoBehaviour
    {
        private bool _previousWireValue;
        private CablePlaceable _startPlaceable = null!;
        private CablePlaceable _endPlaceable = null!;

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
            _endPlaceable = end;

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
            _lineRenderer.SetPositions(new []
            {
                ModifyPosition(_targetA.position),
                ModifyPosition(_targetB.position),
            });

            // get placeable to check by checking if start is not connected
            // if start is not connected then end must be used instead
            CablePlaceable checkingPlaceable = _startPlaceable.Pin.ConnectedPoint != null ? _startPlaceable : _endPlaceable;

            // if there is still nothing, return
            if (checkingPlaceable.Pin.ConnectedPoint == null)
                return;

            // peek at value on wire
            bool watchingValue = PowerManager.GetHasPower(checkingPlaceable.Pin);

            // if the value has changed
            if (watchingValue != _previousWireValue)
            {
                // set the wire colour
                SetColour(watchingValue ? _startPlaceable.PoweredCableColor : _startPlaceable.CableColor);

                _previousWireValue = watchingValue;
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