using ButterBoard.FloatingGrid;
using UnityEngine;

namespace ButterBoard.Simulation.Elements
{
    public class ToggleElement : TickableBehaviour
    {
        private bool _toggleValue = false;

        [SerializeField]
        private GridPin powerPin0 = null!;

        [SerializeField]
        private GridPin powerPin1 = null!;

        [SerializeField]
        private GridPin outputPin0 = null!;

        [SerializeField]
        private GridPin outputPin1 = null!;

        public void OnToggleClick()
        {
            _toggleValue = !_toggleValue;
        }

        public override void PushValues()
        {
            // cannot be optimized to only check if _toggleValue changes as input power could change at any time
            PowerManager.SetPowerState(outputPin0, _toggleValue && PowerManager.GetHasPower(powerPin0));
            PowerManager.SetPowerState(outputPin1, _toggleValue && PowerManager.GetHasPower(powerPin1));
        }
    }
}