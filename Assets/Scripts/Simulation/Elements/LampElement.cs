using UnityEngine;

namespace ButterBoard.Simulation.Elements
{
    public class LampElement : TickableBehaviour
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

        protected override void Tick()
        {
            _hasPower = GetHasPower();
        }
    }
}