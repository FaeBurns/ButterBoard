using System;
using UnityEngine;

namespace ButterBoard
{
    [Serializable]
    public class LerpSettings
    {
        [field: SerializeField]
        public float TranslateLerp { get; private set; }

        [field: SerializeField]
        public float RotateLerp { get; private set; }

        [field: SerializeField]
        public float FinalizeScalarTime { get; private set; }

        public LerpSettings()
        {
            TranslateLerp = 0.9f;
            RotateLerp = 0.9f;
            FinalizeScalarTime = 0.25f;
        }

        public LerpSettings(float translateLerp, float rotateLerp, float finalizeScalarTime)
        {
            TranslateLerp = translateLerp;
            RotateLerp = rotateLerp;
            FinalizeScalarTime = finalizeScalarTime;
        }
    }
}