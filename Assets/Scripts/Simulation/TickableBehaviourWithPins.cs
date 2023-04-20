using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid;
using UnityEngine;

namespace ButterBoard.Simulation
{
    public abstract class TickableBehaviourWithPins : TickableBehaviour, ITickableObject
    {
        private bool[] indexedPinOutputValues = null!;

        [SerializeField]
        protected GridPin powerPin = null!;

        [SerializeField]
        protected GridPin[] indexedPins = Array.Empty<GridPin>();

        protected int IndexedPinCount => indexedPins.Length;

        protected override void Awake()
        {
            base.Awake();

            indexedPinOutputValues = new bool [indexedPins.Length];
        }

        protected bool GetHasPower()
        {
            // allow if powerPin is not set or power is set
            // means that powerPin does not have to be set - TickableBehaviours that might not want a power pin are still allowed to work
            return powerPin == null || PowerManager.GetHasPower(powerPin);
        }

        protected void SetIndexedPinValue(int pinIndex, bool value)
        {
            // enqueue power change
            PowerManager.SetPowerState(indexedPins[pinIndex], value);
        }

        protected void SetIndexedPinValues(int startPinIndex, int endPinIndex, bool[] values)
        {
            if (values.Length != (endPinIndex - startPinIndex) + 1)
                throw new ArgumentException("length of values input must match length of pin range", nameof(values));

            for (int i = startPinIndex; i < indexedPins.Length; i++)
            {
                // enqueue power change
                PowerManager.SetPowerState(indexedPins[i], values[i]);

                if (i == endPinIndex)
                    return;
            }

            throw new IndexOutOfRangeException();
        }

        protected bool GetIndexedPinValue(int pinIndex)
        {
            return PowerManager.GetHasPower(indexedPins[pinIndex]);
        }

        protected bool[] GetIndexedPinValues(int startPinIndex, int endPinIndex)
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
}