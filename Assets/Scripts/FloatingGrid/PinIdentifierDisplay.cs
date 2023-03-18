using System;
using TMPro;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class PinIdentifierDisplay : MonoBehaviour
    {
        private RectTransform _rectTransform = null!;

        [SerializeField]
        private PinIdentifierSide side = PinIdentifierSide.LEFT;

        [SerializeField]
        private float rotation = 45;

        private void Awake()
        {
            TMP_Text text = GetComponentInChildren<TMP_Text>(true);
            _rectTransform = GetComponent<RectTransform>();

            switch (side)
            {
                case PinIdentifierSide.LEFT:
                    _rectTransform.rotation = Quaternion.Euler(0, 0, -rotation);
                    text.alignment = TextAlignmentOptions.MidlineRight;
                    break;
                case PinIdentifierSide.RIGHT:
                    _rectTransform.rotation = Quaternion.Euler(0, 0, rotation);
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            text.text = text.text.Replace(' ', '_').ToUpper();
        }

        private void Update()
        {
            switch (side)
            {
                case PinIdentifierSide.LEFT:
                    _rectTransform.rotation = Quaternion.Euler(0, 0, -rotation);
                    break;
                case PinIdentifierSide.RIGHT:
                    _rectTransform.rotation = Quaternion.Euler(0, 0, rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum PinIdentifierSide
    {
        LEFT,
        RIGHT,
    }
}