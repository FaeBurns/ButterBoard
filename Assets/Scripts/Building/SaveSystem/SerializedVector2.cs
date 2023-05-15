using System;
using UnityEngine;

namespace ButterBoard.Building.SaveSystem
{
    [Serializable]
    public class SerializedVector2
    {
        public float x;
        public float y;
        
        public SerializedVector2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
    }
}