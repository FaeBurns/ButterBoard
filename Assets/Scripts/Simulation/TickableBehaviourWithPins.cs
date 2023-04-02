using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid;
using UnityEngine;

namespace ButterBoard.Simulation
{
    public abstract class TickableBehaviourWithPins : TickableBehaviour, ITickableObject
    {
        private readonly Queue<PowerStatusChange> _powerQueue = new Queue<PowerStatusChange>();

        [SerializeField]
        protected GridPin powerPin = null!;

        [SerializeField]
        private GridPin[] indexedPins = Array.Empty<GridPin>();

        public int IndexedPinCount => indexedPins.Length;

        protected bool GetHasPower()
        {
            // allow if powerPin is not set or power is set
            // means that powerPin does not have to be set - TickableBehaviours that might not want a power pin are still allowed to work
            return powerPin == null || PowerManager.GetHasPower(powerPin);
        }

        public override void DoTick()
        {
            // skip tick if no power is provided to the object
            if (!GetHasPower())
                return;
        }

        public override void PushValues()
        {
            // loop through all in _powerQueue
            while (_powerQueue.Count > 0)
            {
                PowerStatusChange statusChange = _powerQueue.Dequeue();
                GridPin changingPin = indexedPins[statusChange.TargetPinIndex];

                // either power or de-power pin based on status change event
                PowerManager.SetPowerState(changingPin, statusChange.NewPowerState);
            }
        }

        protected void SetPin(int pinIndex, bool value)
        {
            // enqueue power change
            _powerQueue.Enqueue(new PowerStatusChange(pinIndex, value));
        }

        protected void SetPins(int startPinIndex, int endPinIndex, bool value)
        {
            for (int i = startPinIndex; i < indexedPins.Length; i++)
            {
                // enqueue power change
                _powerQueue.Enqueue(new PowerStatusChange(i, value));

                if (i == endPinIndex)
                    return;
            }

            throw new IndexOutOfRangeException();
        }

        protected bool GetPin(int pinIndex)
        {
            return PowerManager.GetHasPower(indexedPins[pinIndex]);
        }

        protected bool[] GetPins(int startPinIndex, int endPinIndex)
        {
            bool[] resultArray = new bool[(endPinIndex - startPinIndex) + 1];
            for (int i = startPinIndex; i < indexedPins.Length; i++)
            {
                resultArray[i] = PowerManager.GetHasPower(indexedPins[i]);

                if (i == endPinIndex)
                    return resultArray;
            }

            throw new IndexOutOfRangeException();
        }
    }

    public class PowerStatusChange
    {
        public int TargetPinIndex { get; }
        public bool NewPowerState { get; }

        public PowerStatusChange(int targetPinIndex, bool newPowerState)
        {
            TargetPinIndex = targetPinIndex;
            NewPowerState = newPowerState;
        }
    }
}