namespace ButterBoard.Simulation.TickableObjects
{
    public class Toggle : TickableBehaviourWithPins
    {
        private bool _actualToggleValue = false;
        private bool _waitingToggleValue = false;

        public void OnToggleClick()
        {
            _waitingToggleValue = !_waitingToggleValue;
        }

        protected override void Tick()
        {
            _actualToggleValue = _waitingToggleValue;

            // set pins 0 and 1 if they should be enabled
            SetPins(0, 1, _actualToggleValue);
        }
    }
}