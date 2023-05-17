using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.Simulation.Elements
{
    public class DigitDisplayElement : TickableBehaviourWithPins
    {
        private bool[] _indexedPinValues = new bool[8];

        [SerializeField]
        private TMP_Text digitDisplayText = null!;

        public override void DoTick()
        {
            _indexedPinValues = GetIndexedPinValues(0, 7);
        }

        private void Update()
        {
            // initialize with first digit value
            int displayValue = 0;

            // set bits on int
            for (int i = 0; i < _indexedPinValues.Length; i++)
            {
                if (_indexedPinValues[i])
                    displayValue += 1 << i;
            }

            string digitText = displayValue.ToString();

            // add any missing leading zeroes
            int missingZeroes = 3 - digitText.Length;
            for (int i = 0; i < missingZeroes; i++)
            {
                digitText = "0" + digitText;
            }

            digitDisplayText.SetText(digitText);
        }

        public void SetTextRotation()
        {
            digitDisplayText.transform.rotation = Quaternion.identity;
        }
    }
}