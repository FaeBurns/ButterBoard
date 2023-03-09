using System;
using UnityEngine;

namespace ButterBoard
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; } = null!;

        public SingletonBehaviour()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Singleton already exists");
            }

            Instance = (T)this;
        }

        private void OnDestroy()
        {
            Instance = null!;
        }
    }
}