using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard
{
    public class SpriteTintHelper : MonoBehaviour
    {
        private readonly Dictionary<SpriteRenderer, Color> _baseColors = new Dictionary<SpriteRenderer, Color>();

        [SerializeField]
        private SpriteRenderer[] spriteRenderers = null!;

        [SerializeField]
        private TintModes tintMode = TintModes.MULTIPLICATIVE;

        private void Awake()
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                _baseColors.Add(spriteRenderer, spriteRenderer.color);
            }
        }

        public void SetTint(Color tintColor)
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = GetColor(_baseColors[spriteRenderer], tintColor);
            }
        }

        public void Override(Color newColor)
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = newColor;
            }
        }

        public void RestoreColor()
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = _baseColors[spriteRenderer];
            }
        }

        private Color GetColor(Color baseColor, Color tintColor)
        {
            return tintMode switch
            {
                TintModes.ADDITIVE => baseColor + tintColor,
                TintModes.MULTIPLICATIVE => baseColor * tintColor,
                TintModes.OVERRIDE => tintColor,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public enum TintModes
    {
        ADDITIVE,
        MULTIPLICATIVE,
        OVERRIDE,
    }
}