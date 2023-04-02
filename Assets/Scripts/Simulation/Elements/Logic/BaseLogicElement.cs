using System;
using ButterBoard.FloatingGrid;
using UnityEngine;

namespace ButterBoard.Simulation.Elements.Logic
{
    public abstract class BaseLogicElement : TickableBehaviour
    {
        private bool _inputStatus = false;

        [SerializeField]
        public GridPin[] inputPins = Array.Empty<GridPin>();

        [SerializeField]
        public GridPin outputPin = null!;

        public override void DoTick()
        {
            bool[] logicInput = new bool[inputPins.Length];

            for (int i = 0; i < inputPins.Length; i++)
            {
                logicInput[i] = PowerManager.GetHasPower(inputPins[i]);
            }

            _inputStatus = GetLogicValue(logicInput);
        }

        public override void PushValues()
        {
            PowerManager.SetPowerState(outputPin, _inputStatus);
        }

        protected abstract bool GetLogicValue(bool[] inputValues);
    }

}