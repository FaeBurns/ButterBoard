using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid.Placement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.FloatingGrid
{
    public class PinIdentifierHoverTrigger : MonoBehaviour
    {
        private GridPin[] _previousPins = Array.Empty<GridPin>();

        [SerializeField]
        private float checkRadius = 1f;

        private void Update()
        {
            if (PlacementManager.Instance.Placing)
            {
                foreach (GridPin pin in _previousPins)
                {
                    // only disable display if the host of the pin that's being disabled is _not_ the current placing object
                    if (pin.Host != PlacementManager.Instance.ActiveService!.GetPlaceable())
                        pin.PinIdentifierDisplay.gameObject.SetActive(false);
                }
                return;
            }

            // get all grid pins in range of cursor
            Vector2 mouseWorldPos = PlacementHelpers.GetMouseWorldPosition();

            // only populate pins list if mouse is not over UI
            List<GridPin> pins =
                EventSystem.current.IsPointerOverGameObject() ?
                    new List<GridPin>() :
                    PlacementHelpers.GetOverlaps<GridPin>(mouseWorldPos, new Vector2(checkRadius, checkRadius), 0);

            // disable all pins used previously
            foreach (GridPin pin in _previousPins)
            {
                // only disable if not in pins
                if (!pins.Contains(pin))
                    pin.PinIdentifierDisplay.gameObject.SetActive(false);
            }

            // enable all pins found this frame
            foreach (GridPin pin in pins)
            {
                pin.PinIdentifierDisplay.gameObject.SetActive(true);
            }

            // save current pins for next frame
            _previousPins = pins.ToArray();
        }
    }
}