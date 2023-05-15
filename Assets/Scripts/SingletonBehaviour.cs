using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ButterBoard
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; } = null!;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.Log($"Instance of {Instance.name} already exists during SingletonBehaviour constructor");
                Destroy(Instance.gameObject);
            }

            Instance = (T)this;
        }

        private void OnDestroy()
        {
            if (Instance != null)
                Debug.Log($"Destroying singleton {Instance.name}");
            
            Instance = null!;
        }
    }
}