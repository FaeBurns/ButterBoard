using System;
using System.Reflection;
using UnityEngine;

namespace ButterBoard
{
    public static class Extensions
    {
        public static bool Approximately(this Vector2 vector, Vector2 target)
        {
            return Mathf.Approximately(vector.x, target.x) && Mathf.Approximately(vector.y, target.y);
        }

        public static bool Approximately(this Vector3 vector, Vector3 target, float epsilon = float.Epsilon)
        {
            return Approximately(vector.x, target.x) && Approximately(vector.y, target.y) && Approximately(vector.z, target.z);
        }

        public static Vector3 Rotate(this Vector3 vector, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vector;
        }

        public static Vector3 Mod(this Vector3 vector, float value)
        {
            return new Vector3(vector.x % value, vector.y % value, vector.z % value);
        }

        private static bool Approximately(float a, float b, float epsilon = float.Epsilon)
        {
            return (double) Mathf.Abs(b - a) < (double) Mathf.Max(1E-06f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), epsilon * 8f);
        }
    }
}