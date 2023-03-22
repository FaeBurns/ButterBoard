namespace ButterBoard.Simulation.Elements
{
    public class ToggleElement : TickableBehaviourWithPins
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

            // set pin 0 if it should be enabled
            SetPin(0, _actualToggleValue);
        }
    }
}