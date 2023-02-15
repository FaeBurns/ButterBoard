using System;
using UnityEngine;

namespace ButterBoard
{
    [Serializable]
    public class LerpSettings
    {
        [field: SerializeField]
        public float TranslateLerp { get; set; }

        [field: SerializeField]
        public float RotateLerp { get; set; }

        public LerpSettings()
        {
            TranslateLerp = 0.9f;
            RotateLerp = 0.9f;
        }

        public LerpSettings(float translateLerp, float rotateLerp)
        {
            TranslateLerp = translateLerp;
            RotateLerp = rotateLerp;
        }
    }
}