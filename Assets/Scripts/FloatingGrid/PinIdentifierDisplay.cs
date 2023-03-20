using System;
using ButterBoard.FloatingGrid.Placement.Placeables;
using TMPro;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class PinIdentifierDisplay : MonoBehaviour
    {
        private RectTransform _rectTransform = null!;
        private TMP_Text _text = null!;
        private Transform _hostPlaceableCenter = null!;

        [SerializeField]
        private float rotation = 45;

        private void OnValidate()
        {
            Awake();
        }

        private void Awake()
        {
            _text = GetComponentInChildren<TMP_Text>(true);
            _rectTransform = GetComponent<RectTransform>();
            BasePlaceable placeable = GetComponentInParent<BasePlaceable>();

            if (placeable == null)
                return;

            _hostPlaceableCenter = placeable.transform;

            _text.text = _text.text.Replace(' ', '_').ToUpper();

            Update();
        }

        private void Update()
        {
            bool xGreater = transform.position.x > _hostPlaceableCenter.position.x + 0.1f;
            bool yGreater = transform.position.y > _hostPlaceableCenter.position.y + 0.1f;

            // right on right side,
            // left on left side
            PinIdentifierSide chosenSide = xGreater ? PinIdentifierSide.RIGHT : PinIdentifierSide.LEFT;

            // 45 in top right and bottom left, negative otherwise
            float angle = xGreater == yGreater ? rotation : -rotation;

            _rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            switch (chosenSide)
            {
                case PinIdentifierSide.LEFT:
                    _text.alignment = TextAlignmentOptions.MidlineRight;
                    _text.margin = new Vector4(-0.25f, 0, 0.25f, 0);
                    break;
                case PinIdentifierSide.RIGHT:
                    _text.alignment = TextAlignmentOptions.MidlineLeft;
                    _text.margin = new Vector4(0.25f, 0, 0, 0);
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