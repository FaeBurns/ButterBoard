using UnityEngine;

namespace ButterBoard.Simulation.Elements
{
    public class LampElement : TickableBehaviourWithPins
    {
        private bool previousPowerState = false;
        private bool _hasPower = false;

        [SerializeField]
        private SpriteTintHelper tintHelper = null!;

        [SerializeField]
        private Color unpoweredColor = Color.white;

        [SerializeField]
        private Color poweredColor = Color.green;

        private void Update()
        {
            if (_hasPower != previousPowerState)
            {
                Color selectedColor = _hasPower ? poweredColor : unpoweredColor;
                tintHelper.SetTint(selectedColor);
            }

            previousPowerState = _hasPower;
        }

        public override void DoTick()
        {
            _hasPower = GetHasPower();
        }
    }
}