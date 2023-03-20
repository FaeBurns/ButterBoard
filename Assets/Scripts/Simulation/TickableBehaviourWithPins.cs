using System;
using ButterBoard.FloatingGrid;
using Coil;
using UnityEngine;

namespace ButterBoard.Simulation
{
    public abstract class TickableBehaviourWithPins : TickableBehaviour, ITickableObject
    {
        [SerializeField]
        private GridPin[] indexedPins = Array.Empty<GridPin>();

        [SerializeField]
        private bool[] indexedPinValues = Array.Empty<bool>();

        public int IndexedPinCount => indexedPins.Length;

        protected override void Awake()
        {
            base.Awake();
            indexedPinValues = new bool[indexedPins.Length];
        }

        public override void DoTick()
        {
            // update indexedPinValues with incoming data
            for (int i = 0; i < indexedPinValues.Length; i++)
            {
                indexedPinValues[i] = indexedPins[i].ConnectedPoint.Wire.Peek().Value;
            }

            // skip tick if no power is provided to the object
            if (!powerPin.ConnectedPoint.Wire.Peek().Value)
                return;

            base.DoTick();
        }

        public override void PushValues()
        {
            for (int i = 0; i < indexedPins.Length; i++)
            {
                if (indexedPinValues[i])
                    indexedPins[i].ConnectedPoint.Wire.Push(new BoolValue(true));
            }
        }

        protected void SetPin(int pinIndex, bool value)
        {
            indexedPinValues[pinIndex] = value;
        }

        protected void SetPins(int startPinIndex, int endPinIndex, bool value)
        {
            for (int i = startPinIndex; i < indexedPinValues.Length; i++)
            {
                indexedPinValues[i] = value;

                if (i == endPinIndex)
                    return;
            }

            throw new InvalidOperationException();
        }

        protected bool GetPin(int pinIndex)
        {
            return indexedPinValues[pinIndex];
        }

        protected bool[] GetPins(int startPinIndex, int endPinIndex)
        {
            bool[] resultArray = new bool[(endPinIndex - startPinIndex) + 1];
            for (int i = startPinIndex; i < indexedPinValues.Length; i++)
            {
                resultArray[i] = indexedPinValues[i];

                if (i == endPinIndex)
                    return resultArray;
            }

            throw new InvalidOperationException();
        }
    }
}