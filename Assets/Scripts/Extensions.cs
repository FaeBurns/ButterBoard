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

        public static bool Approximately(this Vector3 vector, Vector3 target)
        {
            return Mathf.Approximately(vector.x, target.x) && Mathf.Approximately(vector.y, target.y) && Mathf.Approximately(vector.z, target.z);
        }

        public static Vector3 Rotate(this Vector3 vector, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vector;
        }
    }
}