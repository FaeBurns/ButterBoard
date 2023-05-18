using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard
{
    public static class Extensions
    {
        public static bool Approximately(this Vector2 vector, Vector2 target, float epsilon = float.Epsilon)
        {
            return Approximately_Internal(vector.x, target.x, epsilon) && Approximately_Internal(vector.y, target.y, epsilon);
        }

        public static bool ApproximateDistance(this Vector3 vector, Vector3 target, float epsilon = float.Epsilon)
        {
            Vector3 difference = target - vector;
            float distanceSquared = difference.sqrMagnitude;

            return distanceSquared < epsilon * epsilon;
        }

        public static bool ApproximateDistance(this Vector2 vector, Vector2 target, float epsilon = float.Epsilon)
        {
            Vector2 difference = target - vector;
            float distanceSquared = difference.sqrMagnitude;

            return distanceSquared < epsilon * epsilon;
        }

        public static Vector3 Rotate(this Vector3 vector, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vector;
        }

        public static Vector3 Mod(this Vector3 vector, float value)
        {
            return new Vector3(vector.x % value, vector.y % value, vector.z % value);
        }

        public static Quaternion Add(this Quaternion quaternion, Quaternion other)
        {
            return Quaternion.Euler(quaternion.eulerAngles + other.eulerAngles);
        }

        public static bool Approximately(this float a, float b, float epsilon = float.Epsilon)
        {
            return Approximately_Internal(a, b, epsilon);
        }

        private static bool Approximately_Internal(float a, float b, float epsilon = float.Epsilon)
        {
            return (double) Mathf.Abs(b - a) < (double) Mathf.Max(1E-06f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), epsilon * 8f);
        }
    }
}