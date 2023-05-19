using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.Simulation.Elements
{
    public class DigitInputElement : TickableBehaviourWithPins
    {
        private byte _inputValue;
        private readonly bool[] _cachedInputValues = new bool[8];

        [SerializeField]
        private TMP_InputField inputField = null!;

        protected override void Awake()
        {
            base.Awake();

            inputField.onValueChanged.AddListener(TextChanged);
            inputField.onSubmit.AddListener(TextSubmitted);
            inputField.onDeselect.AddListener(TextSubmitted);
        }

        public void Increment()
        {
            if (_inputValue < Byte.MaxValue)
                SetValue((byte)(_inputValue + 1), true);
            else
                SetValue(0, true);

            // deselect button
            EventSystem.current.SetSelectedGameObject(null!);
        }

        public void Decrement()
        {
            if (_inputValue > 0)
                SetValue((byte)(_inputValue - 1), true);
            else
                SetValue(Byte.MaxValue, true);

            // deselect button
            EventSystem.current.SetSelectedGameObject(null!);
        }

        public override void DoTick()
        {
            SetIndexedPinValues(0, 7, _cachedInputValues);
        }

        private void TextChanged(string text)
        {
            // limit input value to byte range and set value
            if (Int32.TryParse(text, out int textValue))
            {
                byte value = (byte)Math.Clamp(textValue, Byte.MinValue, Byte.MaxValue);
                SetValue(value, false);
            }
        }

        private void TextSubmitted(string text)
        {
            inputField.SetTextWithoutNotify(AddMissingZeroes(text));
        }

        private void SetValue(byte value, bool addMissingZeroes)
        {
            _inputValue = value;
            for (int i = 0; i < 8; i++)
            {
                _cachedInputValues[i] = (value & (1 << i)) != 0;
            }
            
            if (addMissingZeroes)
                inputField.SetTextWithoutNotify(AddMissingZeroes(value.ToString()));
            else
                inputField.SetTextWithoutNotify(value.ToString());
        }

        private string AddMissingZeroes(string input)
        {
            int missingZeroes = 3 - input.Length;
            for (int i = 0; i < missingZeroes; i++)
            {
                input = "0" + input;
            }
            return input;
        }
    }
}